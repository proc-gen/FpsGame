using BepuPhysics.Constraints;
using BepuPhysics;
using BepuUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public struct StaticCharacterMotionFunctions : IOneBodyConstraintFunctions<StaticCharacterMotionPrestep, CharacterMotionAccumulatedImpulse>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ComputeJacobians(in Vector3Wide offsetA, in QuaternionWide basisQuaternion,
            out Matrix3x3Wide basis,
            out Matrix2x3Wide horizontalAngularJacobianA,
            out Vector3Wide verticalAngularJacobianA)
        {
            Matrix3x3Wide.CreateFromQuaternion(basisQuaternion, out basis);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.X, out horizontalAngularJacobianA.X);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.Y, out verticalAngularJacobianA);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.Z, out horizontalAngularJacobianA.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyHorizontalImpulse(in Matrix3x3Wide basis,
            in Matrix2x3Wide angularJacobianA, in Vector2Wide constraintSpaceImpulse,
            in BodyInertiaWide inertiaA,
            ref BodyVelocityWide velocityA)
        {
            Vector3Wide.Scale(basis.X, constraintSpaceImpulse.X, out var linearImpulseAX);
            Vector3Wide.Scale(basis.Z, constraintSpaceImpulse.Y, out var linearImpulseAY);
            Vector3Wide.Add(linearImpulseAX, linearImpulseAY, out var linearImpulseA);
            Vector3Wide.Scale(linearImpulseA, inertiaA.InverseMass, out var linearChangeA);
            Vector3Wide.Add(velocityA.Linear, linearChangeA, out velocityA.Linear);

            Matrix2x3Wide.Transform(constraintSpaceImpulse, angularJacobianA, out var angularImpulseA);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseA, inertiaA.InverseInertiaTensor, out var angularChangeA);
            Vector3Wide.Add(velocityA.Angular, angularChangeA, out velocityA.Angular);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVerticalImpulse(in Matrix3x3Wide basis,
            in Vector3Wide angularJacobianA, in Vector<float> constraintSpaceImpulse,
            in BodyInertiaWide inertiaA,
            ref BodyVelocityWide velocityA)
        {
            Vector3Wide.Scale(basis.Y, constraintSpaceImpulse, out var linearImpulseA);
            Vector3Wide.Scale(linearImpulseA, inertiaA.InverseMass, out var linearChangeA);
            Vector3Wide.Add(velocityA.Linear, linearChangeA, out velocityA.Linear);

            Vector3Wide.Scale(angularJacobianA, constraintSpaceImpulse, out var angularImpulseA);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseA, inertiaA.InverseInertiaTensor, out var angularChangeA);
            Vector3Wide.Add(velocityA.Angular, angularChangeA, out velocityA.Angular);
        }


        public void WarmStart(in Vector3Wide positionA, in QuaternionWide orientationA, in BodyInertiaWide inertiaA, ref StaticCharacterMotionPrestep prestep, ref CharacterMotionAccumulatedImpulse accumulatedImpulses, ref BodyVelocityWide velocityA)
        {
            ComputeJacobians(prestep.OffsetFromCharacter, prestep.SurfaceBasis,
                out var basis, out var horizontalAngularJacobianA, out var verticalAngularJacobianA);
            ApplyHorizontalImpulse(basis, horizontalAngularJacobianA, accumulatedImpulses.Horizontal, inertiaA, ref velocityA);
            ApplyVerticalImpulse(basis, verticalAngularJacobianA, accumulatedImpulses.Vertical, inertiaA, ref velocityA);
        }

        public void Solve(in Vector3Wide positionA, in QuaternionWide orientationA, in BodyInertiaWide inertiaA, float dt, float inverseDt, ref StaticCharacterMotionPrestep prestep, ref CharacterMotionAccumulatedImpulse accumulatedImpulses, ref BodyVelocityWide velocityA)
        {
            ComputeJacobians(prestep.OffsetFromCharacter, prestep.SurfaceBasis,
                out var basis, out var horizontalAngularJacobianA, out var verticalAngularJacobianA);

            Vector2Wide horizontalLinearA;
            Vector3Wide.Dot(basis.X, velocityA.Linear, out horizontalLinearA.X);
            Vector3Wide.Dot(basis.Z, velocityA.Linear, out horizontalLinearA.Y);
            Matrix2x3Wide.TransformByTransposeWithoutOverlap(velocityA.Angular, horizontalAngularJacobianA, out var horizontalAngularA);
            Vector2Wide.Add(horizontalLinearA, horizontalAngularA, out var horizontalVelocity);

            Symmetric3x3Wide.MatrixSandwich(horizontalAngularJacobianA, inertiaA.InverseInertiaTensor, out var inverseHorizontalEffectiveMass);

            inverseHorizontalEffectiveMass.XX += inertiaA.InverseMass;
            inverseHorizontalEffectiveMass.YY += inertiaA.InverseMass;
            Symmetric2x2Wide.InvertWithoutOverlap(inverseHorizontalEffectiveMass, out var horizontalEffectiveMass);

            Vector2Wide horizontalConstraintSpaceVelocityChange;
            horizontalConstraintSpaceVelocityChange.X = prestep.TargetVelocity.X - horizontalVelocity.X;

            horizontalConstraintSpaceVelocityChange.Y = -prestep.TargetVelocity.Y - horizontalVelocity.Y;
            Symmetric2x2Wide.TransformWithoutOverlap(horizontalConstraintSpaceVelocityChange, horizontalEffectiveMass, out var horizontalCorrectiveImpulse);

            var previousHorizontalAccumulatedImpulse = accumulatedImpulses.Horizontal;
            Vector2Wide.Add(accumulatedImpulses.Horizontal, horizontalCorrectiveImpulse, out accumulatedImpulses.Horizontal);
            Vector2Wide.Length(accumulatedImpulses.Horizontal, out var horizontalImpulseMagnitude);

            var dtWide = new Vector<float>(dt);
            var maximumHorizontalImpulse = prestep.MaximumHorizontalForce * dtWide;
            var scale = Vector.Min(Vector<float>.One, maximumHorizontalImpulse / Vector.Max(new Vector<float>(1e-16f), horizontalImpulseMagnitude));
            Vector2Wide.Scale(accumulatedImpulses.Horizontal, scale, out accumulatedImpulses.Horizontal);
            Vector2Wide.Subtract(accumulatedImpulses.Horizontal, previousHorizontalAccumulatedImpulse, out horizontalCorrectiveImpulse);

            ApplyHorizontalImpulse(basis, horizontalAngularJacobianA, horizontalCorrectiveImpulse, inertiaA, ref velocityA);

            Vector3Wide.Dot(basis.Y, velocityA.Linear, out var verticalLinearA);
            Vector3Wide.Dot(velocityA.Angular, verticalAngularJacobianA, out var verticalAngularA);

            var verticalBiasVelocity = Vector.Max(Vector<float>.Zero, prestep.Depth * inverseDt);

            Symmetric3x3Wide.VectorSandwich(verticalAngularJacobianA, inertiaA.InverseInertiaTensor, out var verticalAngularContributionA);
            var inverseVerticalEffectiveMass = verticalAngularContributionA + inertiaA.InverseMass;
            var verticalCorrectiveImpulse = (verticalBiasVelocity - verticalLinearA - verticalAngularA) / inverseVerticalEffectiveMass;

            var previousVerticalAccumulatedImpulse = accumulatedImpulses.Vertical;
            var maximumVerticalImpulse = prestep.MaximumVerticalForce * dtWide;
            accumulatedImpulses.Vertical = Vector.Min(Vector<float>.Zero, Vector.Max(accumulatedImpulses.Vertical + verticalCorrectiveImpulse, -maximumVerticalImpulse));
            verticalCorrectiveImpulse = accumulatedImpulses.Vertical - previousVerticalAccumulatedImpulse;

            ApplyVerticalImpulse(basis, verticalAngularJacobianA, verticalCorrectiveImpulse, inertiaA, ref velocityA);
        }


        public bool RequiresIncrementalSubstepUpdates => true;
        public void IncrementallyUpdateForSubstep(in Vector<float> dt, in BodyVelocityWide velocityA, ref StaticCharacterMotionPrestep prestep)
        {
            Vector3Wide.CrossWithoutOverlap(velocityA.Angular, prestep.OffsetFromCharacter, out var wxra);
            Vector3Wide.Add(wxra, velocityA.Linear, out var contactVelocityA);

            var normal = QuaternionWide.TransformUnitY(prestep.SurfaceBasis);
            Vector3Wide.Dot(normal, contactVelocityA, out var estimatedDepthChangeVelocity);
            prestep.Depth -= estimatedDepthChangeVelocity * dt;
        }
    }
}
