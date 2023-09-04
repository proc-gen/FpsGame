using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FpsGame.Common.Physics.Character
{
    public unsafe class CharacterControllers : IDisposable
    {
        public Simulation Simulation { get; private set; }
        BufferPool pool;

        Buffer<int> bodyHandleToCharacterIndex;
        QuickList<CharacterController> characters;

        public int CharacterCount { get { return characters.Count; } }
        public CharacterControllers(BufferPool pool, int initialCharacterCapacity = 4096, int initialBodyHandleCapacity = 4096)
        {
            this.pool = pool;
            characters = new QuickList<CharacterController>(initialCharacterCapacity, pool);
            ResizeBodyHandleCapacity(initialBodyHandleCapacity);
            analyzeContactsWorker = AnalyzeContactsWorker;
            expandBoundingBoxesWorker = ExpandBoundingBoxesWorker;
        }

        public void Initialize(Simulation simulation)
        {
            Simulation = simulation;
            simulation.Solver.Register<DynamicCharacterMotionConstraint>();
            simulation.Solver.Register<StaticCharacterMotionConstraint>();
            simulation.Timestepper.BeforeCollisionDetection += PrepareForContacts;
            simulation.Timestepper.CollisionsDetected += AnalyzeContacts;
        }

        private void ResizeBodyHandleCapacity(int bodyHandleCapacity)
        {
            var oldCapacity = bodyHandleToCharacterIndex.Length;
            pool.ResizeToAtLeast(ref bodyHandleToCharacterIndex, bodyHandleCapacity, bodyHandleToCharacterIndex.Length);
            if (bodyHandleToCharacterIndex.Length > oldCapacity)
            {
                Unsafe.InitBlockUnaligned(ref Unsafe.As<int, byte>(ref bodyHandleToCharacterIndex[oldCapacity]), 0xFF, (uint)((bodyHandleToCharacterIndex.Length - oldCapacity) * sizeof(int)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCharacterIndexForBodyHandle(int bodyHandle)
        {
            Debug.Assert(bodyHandle >= 0 && bodyHandle < bodyHandleToCharacterIndex.Length && bodyHandleToCharacterIndex[bodyHandle] >= 0, "Can only look up indices for body handles associated with characters in this CharacterControllers instance.");
            return bodyHandleToCharacterIndex[bodyHandle];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref CharacterController GetCharacterByIndex(int index)
        {
            return ref characters[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref CharacterController GetCharacterByBodyHandle(BodyHandle bodyHandle)
        {
            Debug.Assert(bodyHandle.Value >= 0 && bodyHandle.Value < bodyHandleToCharacterIndex.Length && bodyHandleToCharacterIndex[bodyHandle.Value] >= 0, "Can only look up indices for body handles associated with characters in this CharacterControllers instance.");
            return ref characters[bodyHandleToCharacterIndex[bodyHandle.Value]];
        }

        public ref CharacterController AllocateCharacter(BodyHandle bodyHandle)
        {
            Debug.Assert(bodyHandle.Value >= 0 && (bodyHandle.Value >= bodyHandleToCharacterIndex.Length || bodyHandleToCharacterIndex[bodyHandle.Value] == -1),
                "Cannot allocate more than one character for the same body handle.");
            if (bodyHandle.Value >= bodyHandleToCharacterIndex.Length)
                ResizeBodyHandleCapacity(Math.Max(bodyHandle.Value + 1, bodyHandleToCharacterIndex.Length * 2));
            var characterIndex = characters.Count;
            ref var character = ref characters.Allocate(pool);
            character = default;
            character.BodyHandle = bodyHandle;
            bodyHandleToCharacterIndex[bodyHandle.Value] = characterIndex;
            return ref character;
        }

        public void RemoveCharacterByIndex(int characterIndex)
        {
            Debug.Assert(characterIndex >= 0 && characterIndex < characters.Count, "Character index must exist in the set of characters.");
            ref var character = ref characters[characterIndex];
            Debug.Assert(character.BodyHandle.Value >= 0 && character.BodyHandle.Value < bodyHandleToCharacterIndex.Length && bodyHandleToCharacterIndex[character.BodyHandle.Value] == characterIndex,
                "Character must exist in the set of characters.");
            bodyHandleToCharacterIndex[character.BodyHandle.Value] = -1;
            characters.FastRemoveAt(characterIndex);

            if (characters.Count > characterIndex)
            {
                bodyHandleToCharacterIndex[characters[characterIndex].BodyHandle.Value] = characterIndex;
            }
        }

        public void RemoveCharacterByBodyHandle(BodyHandle bodyHandle)
        {
            Debug.Assert(bodyHandle.Value >= 0 && bodyHandle.Value < bodyHandleToCharacterIndex.Length && bodyHandleToCharacterIndex[bodyHandle.Value] >= 0,
                "Removing a character by body handle requires that a character associated with the given body handle actually exists.");
            RemoveCharacterByIndex(bodyHandleToCharacterIndex[bodyHandle.Value]);
        }

        struct SupportCandidate
        {
            public Vector3 OffsetFromCharacter;
            public float Depth;
            public Vector3 OffsetFromSupport;
            public Vector3 Normal;
            public CollidableReference Support;
        }

        struct ContactCollectionWorkerCache
        {
            public Buffer<SupportCandidate> SupportCandidates;

            public ContactCollectionWorkerCache(int maximumCharacterCount, BufferPool pool)
            {
                pool.Take(maximumCharacterCount, out SupportCandidates);
                for (int i = 0; i < maximumCharacterCount; ++i)
                {
                    //Initialize the depths to a value that guarantees replacement.
                    SupportCandidates[i].Depth = float.MinValue;
                }
            }

            public void Dispose(BufferPool pool)
            {
                pool.Return(ref SupportCandidates);
            }
        }


        Buffer<ContactCollectionWorkerCache> contactCollectionWorkerCaches;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryReportContacts<TManifold>(CollidableReference characterCollidable, CollidableReference supportCollidable, CollidablePair pair, ref TManifold manifold, int workerIndex) where TManifold : struct, IContactManifold<TManifold>
        {
            if (characterCollidable.Mobility == CollidableMobility.Dynamic && characterCollidable.BodyHandle.Value < bodyHandleToCharacterIndex.Length)
            {
                var characterBodyHandle = characterCollidable.BodyHandle;
                var characterIndex = bodyHandleToCharacterIndex[characterBodyHandle.Value];
                if (characterIndex >= 0)
                {
                    ref var character = ref characters[characterIndex];
                    
                    ref var bodyLocation = ref Simulation.Bodies.HandleToLocation[character.BodyHandle.Value];
                    ref var set = ref Simulation.Bodies.Sets[bodyLocation.SetIndex];
                    ref var pose = ref set.DynamicsState[bodyLocation.Index].Motion.Pose;
                    QuaternionEx.Transform(character.LocalUp, pose.Orientation, out var up);

                    if (manifold.Convex)
                    {
                        ref var convexManifold = ref Unsafe.As<TManifold, ConvexContactManifold>(ref manifold);
                        var normalUpDot = Vector3.Dot(convexManifold.Normal, up);
                        
                        if ((pair.B.Packed == characterCollidable.Packed ? -normalUpDot : normalUpDot) > character.CosMaximumSlope)
                        {
                           var maximumDepth = convexManifold.Contact0.Depth;
                            var maximumDepthIndex = 0;
                            for (int i = 1; i < convexManifold.Count; ++i)
                            {
                                ref var candidateDepth = ref Unsafe.Add(ref convexManifold.Contact0, i).Depth;
                                if (candidateDepth > maximumDepth)
                                {
                                    maximumDepth = candidateDepth;
                                    maximumDepthIndex = i;
                                }
                            }
                            if (maximumDepth >= character.MinimumSupportDepth || (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth))
                            {
                                ref var supportCandidate = ref contactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];
                                if (supportCandidate.Depth < maximumDepth)
                                {
                                    supportCandidate.Depth = maximumDepth;
                                    ref var deepestContact = ref Unsafe.Add(ref convexManifold.Contact0, maximumDepthIndex);
                                    var offsetFromB = deepestContact.Offset - convexManifold.OffsetB;
                                    if (pair.B.Packed == characterCollidable.Packed)
                                    {
                                        supportCandidate.Normal = -convexManifold.Normal;
                                        supportCandidate.OffsetFromCharacter = offsetFromB;
                                        supportCandidate.OffsetFromSupport = deepestContact.Offset;
                                    }
                                    else
                                    {
                                        supportCandidate.Normal = convexManifold.Normal;
                                        supportCandidate.OffsetFromCharacter = deepestContact.Offset;
                                        supportCandidate.OffsetFromSupport = offsetFromB;
                                    }
                                    supportCandidate.Support = supportCollidable;
                                }
                            }
                        }
                    }
                    else
                    {
                        ref var nonconvexManifold = ref Unsafe.As<TManifold, NonconvexContactManifold>(ref manifold);
                        var maximumDepth = float.MinValue;
                        var maximumDepthIndex = -1;
                        for (int i = 0; i < nonconvexManifold.Count; ++i)
                        {
                            ref var candidate = ref Unsafe.Add(ref nonconvexManifold.Contact0, i);
                            if (candidate.Depth > maximumDepth)
                            {
                                var upDot = Vector3.Dot(candidate.Normal, up);
                                if ((pair.B.Packed == characterCollidable.Packed ? -upDot : upDot) > character.CosMaximumSlope)
                                {
                                    maximumDepth = candidate.Depth;
                                    maximumDepthIndex = i;
                                }
                            }
                        }
                        if (maximumDepth >= character.MinimumSupportDepth || (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth))
                        {
                            ref var supportCandidate = ref contactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];
                            if (supportCandidate.Depth < maximumDepth)
                            {
                                ref var deepestContact = ref Unsafe.Add(ref nonconvexManifold.Contact0, maximumDepthIndex);
                                supportCandidate.Depth = maximumDepth;
                                var offsetFromB = deepestContact.Offset - nonconvexManifold.OffsetB;
                                if (pair.B.Packed == characterCollidable.Packed)
                                {
                                    supportCandidate.Normal = -deepestContact.Normal;
                                    supportCandidate.OffsetFromCharacter = offsetFromB;
                                    supportCandidate.OffsetFromSupport = deepestContact.Offset;
                                }
                                else
                                {
                                    supportCandidate.Normal = deepestContact.Normal;
                                    supportCandidate.OffsetFromCharacter = deepestContact.Offset;
                                    supportCandidate.OffsetFromSupport = offsetFromB;
                                }
                                supportCandidate.Support = supportCollidable;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReportContacts<TManifold>(in CollidablePair pair, ref TManifold manifold, int workerIndex, ref PairMaterialProperties materialProperties) where TManifold : struct, IContactManifold<TManifold>
        {
            Debug.Assert(contactCollectionWorkerCaches.Allocated && workerIndex < contactCollectionWorkerCaches.Length && contactCollectionWorkerCaches[workerIndex].SupportCandidates.Allocated,
                "Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?");
            if (manifold.Count == 0)
                return false;

            var aIsCharacter = TryReportContacts(pair.A, pair.B, pair, ref manifold, workerIndex);
            var bIsCharacter = TryReportContacts(pair.B, pair.A, pair, ref manifold, workerIndex);
            if (aIsCharacter || bIsCharacter)
            {
                materialProperties.FrictionCoefficient = 0;
                return true;
            }
            return false;
        }

        Buffer<(int Start, int Count)> boundingBoxExpansionJobs;

        void ExpandBoundingBoxes(int start, int count)
        {
            var end = start + count;
            for (int i = start; i < end; ++i)
            {
                ref var character = ref characters[i];
                var characterBody = Simulation.Bodies[character.BodyHandle];
                if (characterBody.Awake)
                {
                    Simulation.BroadPhase.GetActiveBoundsPointers(characterBody.Collidable.BroadPhaseIndex, out var min, out var max);
                    QuaternionEx.Transform(character.LocalUp, characterBody.Pose.Orientation, out var characterUp);
                    var supportExpansion = character.MinimumSupportContinuationDepth * characterUp;
                    *min += Vector3.Min(Vector3.Zero, supportExpansion);
                    *max += Vector3.Max(Vector3.Zero, supportExpansion);
                }
            }
        }

        int boundingBoxExpansionJobIndex;
        Action<int> expandBoundingBoxesWorker;

        void ExpandBoundingBoxesWorker(int workerIndex)
        {
            while (true)
            {
                var jobIndex = Interlocked.Increment(ref boundingBoxExpansionJobIndex);
                if (jobIndex < boundingBoxExpansionJobs.Length)
                {
                    ref var job = ref boundingBoxExpansionJobs[jobIndex];
                    ExpandBoundingBoxes(job.Start, job.Count);
                }
                else
                {
                    break;
                }
            }
        }

        void PrepareForContacts(float dt, IThreadDispatcher threadDispatcher = null)
        {
            Debug.Assert(!contactCollectionWorkerCaches.Allocated, "Worker caches were already allocated; did you forget to call AnalyzeContacts after collision detection to flush the previous frame's results?");
            var threadCount = threadDispatcher == null ? 1 : threadDispatcher.ThreadCount;
            pool.Take(threadCount, out contactCollectionWorkerCaches);
            for (int i = 0; i < contactCollectionWorkerCaches.Length; ++i)
            {
                contactCollectionWorkerCaches[i] = new ContactCollectionWorkerCache(characters.Count, pool);
            }
            
            if (threadCount == 1 || characters.Count < 256)
            {
                ExpandBoundingBoxes(0, characters.Count);
            }
            else
            {
                var jobCount = Math.Min(characters.Count, threadCount);
                var charactersPerJob = characters.Count / jobCount;
                var baseCharacterCount = charactersPerJob * jobCount;
                var remainder = characters.Count - baseCharacterCount;
                pool.Take(jobCount, out boundingBoxExpansionJobs);
                var previousEnd = 0;
                for (int jobIndex = 0; jobIndex < jobCount; ++jobIndex)
                {
                    var charactersForJob = jobIndex < remainder ? charactersPerJob + 1 : charactersPerJob;
                    ref var job = ref boundingBoxExpansionJobs[jobIndex];
                    job.Start = previousEnd;
                    job.Count = charactersForJob;
                    previousEnd += job.Count;
                }

                boundingBoxExpansionJobIndex = -1;
                threadDispatcher.DispatchWorkers(expandBoundingBoxesWorker, boundingBoxExpansionJobs.Length);
                pool.Return(ref boundingBoxExpansionJobs);
            }
        }

        struct PendingDynamicConstraint
        {
            public int CharacterIndex;
            public DynamicCharacterMotionConstraint Description;
        }
        struct PendingStaticConstraint
        {
            public int CharacterIndex;
            public StaticCharacterMotionConstraint Description;
        }
        struct Jump
        {
            public int CharacterBodyIndex;
            public Vector3 CharacterVelocityChange;
            public int SupportBodyIndex;
            public Vector3 SupportImpulseOffset;
        }

        struct AnalyzeContactsWorkerCache
        {
            public QuickList<ConstraintHandle> ConstraintHandlesToRemove;
            public QuickList<PendingDynamicConstraint> DynamicConstraintsToAdd;
            public QuickList<PendingStaticConstraint> StaticConstraintsToAdd;
            public QuickList<Jump> Jumps;

            public AnalyzeContactsWorkerCache(int maximumCharacterCount, BufferPool pool)
            {
                ConstraintHandlesToRemove = new QuickList<ConstraintHandle>(maximumCharacterCount, pool);
                DynamicConstraintsToAdd = new QuickList<PendingDynamicConstraint>(maximumCharacterCount, pool);
                StaticConstraintsToAdd = new QuickList<PendingStaticConstraint>(maximumCharacterCount, pool);
                Jumps = new QuickList<Jump>(maximumCharacterCount, pool);
            }

            public void Dispose(BufferPool pool)
            {
                ConstraintHandlesToRemove.Dispose(pool);
                DynamicConstraintsToAdd.Dispose(pool);
                StaticConstraintsToAdd.Dispose(pool);
                Jumps.Dispose(pool);
            }
        }

        Buffer<AnalyzeContactsWorkerCache> analyzeContactsWorkerCaches;

        void AnalyzeContactsForCharacterRegion(int start, int exclusiveEnd, int workerIndex)
        {
            ref var analyzeContactsWorkerCache = ref analyzeContactsWorkerCaches[workerIndex];
            for (int characterIndex = start; characterIndex < exclusiveEnd; ++characterIndex)
            {
                ref var character = ref characters[characterIndex];
                ref var bodyLocation = ref Simulation.Bodies.HandleToLocation[character.BodyHandle.Value];
                if (bodyLocation.SetIndex == 0)
                {
                    var supportCandidate = contactCollectionWorkerCaches[0].SupportCandidates[characterIndex];
                    for (int j = 1; j < contactCollectionWorkerCaches.Length; ++j)
                    {
                        ref var workerCandidate = ref contactCollectionWorkerCaches[j].SupportCandidates[characterIndex];
                        if (workerCandidate.Depth > supportCandidate.Depth)
                        {
                            supportCandidate = workerCandidate;
                        }
                    }
                    if (character.Supported)
                    {
                        if (!Simulation.Solver.ConstraintExists(character.MotionConstraintHandle) ||
                            (Simulation.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId != DynamicCharacterMotionTypeProcessor.BatchTypeId &&
                            Simulation.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId != StaticCharacterMotionTypeProcessor.BatchTypeId))
                        {
                            character.Supported = false;
                        }
                     }

                    var shouldRemove = character.Supported && (character.TryJump || supportCandidate.Depth == float.MinValue || character.Support.Packed != supportCandidate.Support.Packed);
                    if (shouldRemove)
                    {
                        analyzeContactsWorkerCache.ConstraintHandlesToRemove.AllocateUnsafely() = character.MotionConstraintHandle;
                    }

                    if (supportCandidate.Depth > float.MinValue && character.TryJump)
                    {
                        QuaternionEx.Transform(character.LocalUp, Simulation.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Pose.Orientation, out var characterUp);

                        var characterUpVelocity = Vector3.Dot(Simulation.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Velocity.Linear, characterUp);
                        
                        if (character.Support.Mobility != CollidableMobility.Static)
                        {
                            ref var supportingBodyLocation = ref Simulation.Bodies.HandleToLocation[character.Support.BodyHandle.Value];
                            Debug.Assert(supportingBodyLocation.SetIndex == 0, "If the character is active, any support should be too.");
                            ref var supportVelocity = ref Simulation.Bodies.ActiveSet.DynamicsState[supportingBodyLocation.Index].Motion.Velocity;
                            var wxr = Vector3.Cross(supportVelocity.Angular, supportCandidate.OffsetFromSupport);
                            var supportContactVelocity = supportVelocity.Linear + wxr;
                            var supportUpVelocity = Vector3.Dot(supportContactVelocity, characterUp);

                            ref var jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
                            jump.CharacterBodyIndex = bodyLocation.Index;
                            jump.CharacterVelocityChange = characterUp * MathF.Max(0, character.JumpVelocity - (characterUpVelocity - supportUpVelocity));
                            if (character.Support.Mobility == CollidableMobility.Dynamic)
                            {
                                jump.SupportBodyIndex = supportingBodyLocation.Index;
                                jump.SupportImpulseOffset = supportCandidate.OffsetFromSupport;
                            }
                            else
                            {
                                jump.SupportBodyIndex = -1;
                            }
                        }
                        else
                        {
                            ref var jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
                            jump.CharacterBodyIndex = bodyLocation.Index;
                            jump.CharacterVelocityChange = characterUp * MathF.Max(0, character.JumpVelocity - characterUpVelocity);
                            jump.SupportBodyIndex = -1;
                        }
                        character.Supported = false;
                    }
                    else if (supportCandidate.Depth > float.MinValue)
                    {
                        Matrix3x3 surfaceBasis;
                        surfaceBasis.Y = supportCandidate.Normal;

                        QuaternionEx.Transform(character.LocalUp, Simulation.Bodies.ActiveSet.DynamicsState[bodyLocation.Index].Motion.Pose.Orientation, out var up);
                        var rayDistance = Vector3.Dot(character.ViewDirection, surfaceBasis.Y);
                        var rayVelocity = Vector3.Dot(up, surfaceBasis.Y);
                        Debug.Assert(rayVelocity > 0,
                            "The calibrated support normal and the character's up direction should have a positive dot product if the maximum slope is working properly. Is the maximum slope >= pi/2?");
                        surfaceBasis.Z = up * (rayDistance / rayVelocity) - character.ViewDirection;
                        var zLengthSquared = surfaceBasis.Z.LengthSquared();
                        if (zLengthSquared > 1e-12f)
                        {
                            surfaceBasis.Z /= MathF.Sqrt(zLengthSquared);
                        }
                        else
                        {
                            QuaternionEx.GetQuaternionBetweenNormalizedVectors(Vector3.UnitY, surfaceBasis.Y, out var rotation);
                            QuaternionEx.TransformUnitZ(rotation, out surfaceBasis.Z);
                        }
                        surfaceBasis.X = Vector3.Cross(surfaceBasis.Y, surfaceBasis.Z);
                        QuaternionEx.CreateFromRotationMatrix(surfaceBasis, out var surfaceBasisQuaternion);
                        if (supportCandidate.Support.Mobility != CollidableMobility.Static)
                        {
                            var motionConstraint = new DynamicCharacterMotionConstraint
                            {
                                MaximumHorizontalForce = character.MaximumHorizontalForce,
                                MaximumVerticalForce = character.MaximumVerticalForce,
                                OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
                                OffsetFromSupportToSupportPoint = supportCandidate.OffsetFromSupport,
                                SurfaceBasis = surfaceBasisQuaternion,
                                TargetVelocity = character.TargetVelocity,
                                Depth = supportCandidate.Depth
                            };
                            if (character.Supported && !shouldRemove)
                            {
                                Simulation.Solver.ApplyDescriptionWithoutWaking(character.MotionConstraintHandle, motionConstraint);
                            }
                            else
                            {
                                ref var pendingConstraint = ref analyzeContactsWorkerCache.DynamicConstraintsToAdd.AllocateUnsafely();
                                pendingConstraint.Description = motionConstraint;
                                pendingConstraint.CharacterIndex = characterIndex;
                            }
                        }
                        else
                        {
                            var motionConstraint = new StaticCharacterMotionConstraint
                            {
                                MaximumHorizontalForce = character.MaximumHorizontalForce,
                                MaximumVerticalForce = character.MaximumVerticalForce,
                                OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
                                SurfaceBasis = surfaceBasisQuaternion,
                                TargetVelocity = character.TargetVelocity,
                                Depth = supportCandidate.Depth
                            };
                            if (character.Supported && !shouldRemove)
                            {
                                Simulation.Solver.ApplyDescriptionWithoutWaking(character.MotionConstraintHandle, motionConstraint);
                            }
                            else
                            {
                                ref var pendingConstraint = ref analyzeContactsWorkerCache.StaticConstraintsToAdd.AllocateUnsafely();
                                pendingConstraint.Description = motionConstraint;
                                pendingConstraint.CharacterIndex = characterIndex;
                            }
                        }
                        character.Supported = true;
                        character.Support = supportCandidate.Support;
                    }
                    else
                    {
                        character.Supported = false;
                    }
                }
                character.TryJump = false;
            }
        }

        struct AnalyzeContactsJob
        {
            public int Start;
            public int ExclusiveEnd;
        }

        int analysisJobIndex;
        int analysisJobCount;
        Buffer<AnalyzeContactsJob> jobs;
        Action<int> analyzeContactsWorker;

        void AnalyzeContactsWorker(int workerIndex)
        {
            int jobIndex;
            while ((jobIndex = Interlocked.Increment(ref analysisJobIndex)) < analysisJobCount)
            {
                ref var job = ref jobs[jobIndex];
                AnalyzeContactsForCharacterRegion(job.Start, job.ExclusiveEnd, workerIndex);
            }
        }

        void AnalyzeContacts(float dt, IThreadDispatcher threadDispatcher)
        {
            Debug.Assert(contactCollectionWorkerCaches.Allocated, "Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?");

            if (threadDispatcher == null)
            {
                pool.Take(1, out analyzeContactsWorkerCaches);
                analyzeContactsWorkerCaches[0] = new AnalyzeContactsWorkerCache(characters.Count, pool);
                AnalyzeContactsForCharacterRegion(0, characters.Count, 0);
            }
            else
            {
                analysisJobCount = Math.Min(characters.Count, threadDispatcher.ThreadCount * 4);
                if (analysisJobCount > 0)
                {
                    pool.Take(threadDispatcher.ThreadCount, out analyzeContactsWorkerCaches);
                    pool.Take(analysisJobCount, out jobs);
                    for (int i = 0; i < threadDispatcher.ThreadCount; ++i)
                    {
                        analyzeContactsWorkerCaches[i] = new AnalyzeContactsWorkerCache(characters.Count, pool);
                    }
                    var baseCount = characters.Count / analysisJobCount;
                    var remainder = characters.Count - baseCount * analysisJobCount;
                    var previousEnd = 0;
                    for (int i = 0; i < analysisJobCount; ++i)
                    {
                        ref var job = ref jobs[i];
                        job.Start = previousEnd;
                        job.ExclusiveEnd = job.Start + (i < remainder ? baseCount + 1 : baseCount);
                        previousEnd = job.ExclusiveEnd;
                    }
                    analysisJobIndex = -1;
                    threadDispatcher.DispatchWorkers(analyzeContactsWorker, analysisJobCount);
                    pool.Return(ref jobs);
                }
            }

            for (int i = 0; i < contactCollectionWorkerCaches.Length; ++i)
            {
                contactCollectionWorkerCaches[i].Dispose(pool);
            }
            pool.Return(ref contactCollectionWorkerCaches);

            if (analyzeContactsWorkerCaches.Allocated)
            {
                for (int threadIndex = 0; threadIndex < analyzeContactsWorkerCaches.Length; ++threadIndex)
                {
                    ref var cache = ref analyzeContactsWorkerCaches[threadIndex];
                    for (int i = 0; i < cache.ConstraintHandlesToRemove.Count; ++i)
                    {
                        Simulation.Solver.Remove(cache.ConstraintHandlesToRemove[i]);
                    }
                }
                for (int threadIndex = 0; threadIndex < analyzeContactsWorkerCaches.Length; ++threadIndex)
                {
                    ref var workerCache = ref analyzeContactsWorkerCaches[threadIndex];
                    for (int i = 0; i < workerCache.StaticConstraintsToAdd.Count; ++i)
                    {
                        ref var pendingConstraint = ref workerCache.StaticConstraintsToAdd[i];
                        ref var character = ref characters[pendingConstraint.CharacterIndex];
                        Debug.Assert(character.Support.Mobility == CollidableMobility.Static);
                        character.MotionConstraintHandle = Simulation.Solver.Add(character.BodyHandle, pendingConstraint.Description);
                    }
                    for (int i = 0; i < workerCache.DynamicConstraintsToAdd.Count; ++i)
                    {
                        ref var pendingConstraint = ref workerCache.DynamicConstraintsToAdd[i];
                        ref var character = ref characters[pendingConstraint.CharacterIndex];
                        Debug.Assert(character.Support.Mobility != CollidableMobility.Static);
                        character.MotionConstraintHandle = Simulation.Solver.Add(character.BodyHandle, character.Support.BodyHandle, pendingConstraint.Description);
                    }
                    ref var activeSet = ref Simulation.Bodies.ActiveSet;
                    for (int i = 0; i < workerCache.Jumps.Count; ++i)
                    {
                        ref var jump = ref workerCache.Jumps[i];
                        activeSet.DynamicsState[jump.CharacterBodyIndex].Motion.Velocity.Linear += jump.CharacterVelocityChange;
                        if (jump.SupportBodyIndex >= 0)
                        {
                            BodyReference.ApplyImpulse(Simulation.Bodies.ActiveSet, jump.SupportBodyIndex, jump.CharacterVelocityChange / -activeSet.DynamicsState[jump.CharacterBodyIndex].Inertia.Local.InverseMass, jump.SupportImpulseOffset);
                        }
                    }
                    workerCache.Dispose(pool);
                }
                pool.Return(ref analyzeContactsWorkerCaches);
            }
        }

        public void EnsureCapacity(int characterCapacity, int bodyHandleCapacity)
        {
            characters.EnsureCapacity(characterCapacity, pool);
            if (bodyHandleToCharacterIndex.Length < bodyHandleCapacity)
            {
                ResizeBodyHandleCapacity(bodyHandleCapacity);
            }
        }

        public void Resize(int characterCapacity, int bodyHandleCapacity)
        {
            int lastOccupiedIndex = -1;
            for (int i = bodyHandleToCharacterIndex.Length - 1; i >= 0; --i)
            {
                if (bodyHandleToCharacterIndex[i] != -1)
                {
                    lastOccupiedIndex = i;
                    break;
                }
            }
            var targetHandleCapacity = BufferPool.GetCapacityForCount<int>(Math.Max(lastOccupiedIndex + 1, bodyHandleCapacity));
            if (targetHandleCapacity != bodyHandleToCharacterIndex.Length)
                ResizeBodyHandleCapacity(targetHandleCapacity);

            var targetCharacterCapacity = BufferPool.GetCapacityForCount<int>(Math.Max(characters.Count, characterCapacity));
            if (targetCharacterCapacity != characters.Span.Length)
                characters.Resize(targetCharacterCapacity, pool);
        }

        bool disposed;
        
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Simulation.Timestepper.BeforeCollisionDetection -= PrepareForContacts;
                Simulation.Timestepper.CollisionsDetected -= AnalyzeContacts;
                characters.Dispose(pool);
                pool.Return(ref bodyHandleToCharacterIndex);
            }
        }
    }
}