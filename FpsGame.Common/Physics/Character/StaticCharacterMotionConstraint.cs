using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using BepuUtilities;
using System;
using System.Numerics;
using static BepuUtilities.GatherScatter;


namespace FpsGame.Common.Physics.Character
{
    public struct StaticCharacterMotionConstraint : IOneBodyConstraintDescription<StaticCharacterMotionConstraint>
    {
        public float MaximumHorizontalForce;
        public float MaximumVerticalForce;
        public Vector2 TargetVelocity;
        public float Depth;
        public Quaternion SurfaceBasis;
        public Vector3 OffsetFromCharacterToSupportPoint;

        public int ConstraintTypeId => StaticCharacterMotionTypeProcessor.BatchTypeId;

        public Type TypeProcessorType => typeof(StaticCharacterMotionTypeProcessor);
        public TypeProcessor CreateTypeProcessor() => new StaticCharacterMotionTypeProcessor();

        public void ApplyDescription(ref TypeBatch batch, int bundleIndex, int innerIndex)
        {
            ref var target = ref GetOffsetInstance(ref Buffer<StaticCharacterMotionPrestep>.Get(ref batch.PrestepData, bundleIndex), innerIndex);
            QuaternionWide.WriteFirst(SurfaceBasis, ref target.SurfaceBasis);
            GetFirst(ref target.MaximumHorizontalForce) = MaximumHorizontalForce;
            GetFirst(ref target.MaximumVerticalForce) = MaximumVerticalForce;
            Vector2Wide.WriteFirst(TargetVelocity, ref target.TargetVelocity);
            GetFirst(ref target.Depth) = Depth;
            Vector3Wide.WriteFirst(OffsetFromCharacterToSupportPoint, ref target.OffsetFromCharacter);
        }

        public void BuildDescription(ref TypeBatch batch, int bundleIndex, int innerIndex, out StaticCharacterMotionConstraint description)
        {
            ref var source = ref GetOffsetInstance(ref Buffer<StaticCharacterMotionPrestep>.Get(ref batch.PrestepData, bundleIndex), innerIndex);
            QuaternionWide.ReadFirst(source.SurfaceBasis, out description.SurfaceBasis);
            description.MaximumHorizontalForce = GetFirst(ref source.MaximumHorizontalForce);
            description.MaximumVerticalForce = GetFirst(ref source.MaximumVerticalForce);
            Vector2Wide.ReadFirst(source.TargetVelocity, out description.TargetVelocity);
            description.Depth = GetFirst(ref source.Depth);
            Vector3Wide.ReadFirst(source.OffsetFromCharacter, out description.OffsetFromCharacterToSupportPoint);
        }
    }
}
