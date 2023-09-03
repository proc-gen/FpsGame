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
    public struct DynamicCharacterMotionFunctions : ITwoBodyConstraintFunctions<DynamicCharacterMotionPrestep, CharacterMotionAccumulatedImpulse>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ComputeJacobians(in Vector3Wide offsetA, in Vector3Wide offsetB, in QuaternionWide basisQuaternion,
            out Matrix3x3Wide basis,
            out Matrix2x3Wide horizontalAngularJacobianA, out Matrix2x3Wide horizontalAngularJacobianB,
            out Vector3Wide verticalAngularJacobianA, out Vector3Wide verticalAngularJacobianB)
        {
            Matrix3x3Wide.CreateFromQuaternion(basisQuaternion, out basis);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.X, out horizontalAngularJacobianA.X);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.Y, out verticalAngularJacobianA);
            Vector3Wide.CrossWithoutOverlap(offsetA, basis.Z, out horizontalAngularJacobianA.Y);
            Vector3Wide.CrossWithoutOverlap(basis.X, offsetB, out horizontalAngularJacobianB.X);
            Vector3Wide.CrossWithoutOverlap(basis.Y, offsetB, out verticalAngularJacobianB);
            Vector3Wide.CrossWithoutOverlap(basis.Z, offsetB, out horizontalAngularJacobianB.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyHorizontalImpulse(in Matrix3x3Wide basis,
            in Matrix2x3Wide angularJacobianA, in Matrix2x3Wide angularJacobianB, in Vector2Wide constraintSpaceImpulse,
            in BodyInertiaWide inertiaA, in BodyInertiaWide inertiaB,
            ref BodyVelocityWide velocityA, ref BodyVelocityWide velocityB)
        {
            Vector3Wide.Scale(basis.X, constraintSpaceImpulse.X, out var linearImpulseAX);
            Vector3Wide.Scale(basis.Z, constraintSpaceImpulse.Y, out var linearImpulseAY);
            Vector3Wide.Add(linearImpulseAX, linearImpulseAY, out var linearImpulseA);
            Vector3Wide.Scale(linearImpulseA, inertiaA.InverseMass, out var linearChangeA);
            Vector3Wide.Add(velocityA.Linear, linearChangeA, out velocityA.Linear);
            Vector3Wide.Scale(linearImpulseA, inertiaB.InverseMass, out var negatedLinearChangeB);
            Vector3Wide.Subtract(velocityB.Linear, negatedLinearChangeB, out velocityB.Linear);

            Matrix2x3Wide.Transform(constraintSpaceImpulse, angularJacobianA, out var angularImpulseA);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseA, inertiaA.InverseInertiaTensor, out var angularChangeA);
            Vector3Wide.Add(velocityA.Angular, angularChangeA, out velocityA.Angular);
            Matrix2x3Wide.Transform(constraintSpaceImpulse, angularJacobianB, out var angularImpulseB);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseB, inertiaB.InverseInertiaTensor, out var angularChangeB);
            Vector3Wide.Add(velocityB.Angular, angularChangeB, out velocityB.Angular);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVerticalImpulse(in Matrix3x3Wide basis,
            in Vector3Wide angularJacobianA, in Vector3Wide angularJacobianB, in Vector<float> constraintSpaceImpulse,
            in BodyInertiaWide inertiaA, in BodyInertiaWide inertiaB,
            ref BodyVelocityWide velocityA, ref BodyVelocityWide velocityB)
        {
            Vector3Wide.Scale(basis.Y, constraintSpaceImpulse, out var linearImpulseA);
            Vector3Wide.Scale(linearImpulseA, inertiaA.InverseMass, out var linearChangeA);
            Vector3Wide.Add(velocityA.Linear, linearChangeA, out velocityA.Linear);
            Vector3Wide.Scale(linearImpulseA, inertiaB.InverseMass, out var negatedLinearChangeB);
            Vector3Wide.Subtract(velocityB.Linear, negatedLinearChangeB, out velocityB.Linear);

            Vector3Wide.Scale(angularJacobianA, constraintSpaceImpulse, out var angularImpulseA);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseA, inertiaA.InverseInertiaTensor, out var angularChangeA);
            Vector3Wide.Add(velocityA.Angular, angularChangeA, out velocityA.Angular);
            Vector3Wide.Scale(angularJacobianB, constraintSpaceImpulse, out var angularImpulseB);
            Symmetric3x3Wide.TransformWithoutOverlap(angularImpulseB, inertiaB.InverseInertiaTensor, out var angularChangeB);
            Vector3Wide.Add(velocityB.Angular, angularChangeB, out velocityB.Angular);
        }


        public void WarmStart(in Vector3Wide positionA, in QuaternionWide orientationA, in BodyInertiaWide inertiaA, in Vector3Wide positionB, in QuaternionWide orientationB, in BodyInertiaWide inertiaB, ref DynamicCharacterMotionPrestep prestep, ref CharacterMotionAccumulatedImpulse accumulatedImpulses, ref BodyVelocityWide velocityA, ref BodyVelocityWide velocityB)
        {
            ComputeJacobians(prestep.OffsetFromCharacter, prestep.OffsetFromSupport, prestep.SurfaceBasis,
                out var basis, out var horizontalAngularJacobianA, out var horizontalAngularJacobianB, out var verticalAngularJacobianA, out var verticalAngularJacobianB);
            ApplyHorizontalImpulse(basis, horizontalAngularJacobianA, horizontalAngularJacobianB, accumulatedImpulses.Horizontal, inertiaA, inertiaB, ref velocityA, ref velocityB);
            ApplyVerticalImpulse(basis, verticalAngularJacobianA, verticalAngularJacobianB, accumulatedImpulses.Vertical, inertiaA, inertiaB, ref velocityA, ref velocityB);
        }

        public void Solve(in Vector3Wide positionA, in QuaternionWide orientationA, in BodyInertiaWide inertiaA, in Vector3Wide positionB, in QuaternionWide orientationB, in BodyInertiaWide inertiaB, float dt, float inverseDt, ref DynamicCharacterMotionPrestep prestep, ref CharacterMotionAccumulatedImpulse accumulatedImpulses, ref BodyVelocityWide velocityA, ref BodyVelocityWide velocityB)
        {
            ComputeJacobians(prestep.OffsetFromCharacter, prestep.OffsetFromSupport, prestep.SurfaceBasis,
                out var basis, out var horizontalAngularJacobianA, out var horizontalAngularJacobianB, out var verticalAngularJacobianA, out var verticalAngularJacobianB);

            Vector2Wide horizontalLinearA;
            Vector3Wide.Dot(basis.X, velocityA.Linear, out horizontalLinearA.X);
            Vector3Wide.Dot(basis.Z, velocityA.Linear, out horizontalLinearA.Y);
            Matrix2x3Wide.TransformByTransposeWithoutOverlap(velocityA.Angular, horizontalAngularJacobianA, out var horizontalAngularA);
            Vector2Wide negatedHorizontalLinearB;
            Vector3Wide.Dot(basis.X, velocityB.Linear, out negatedHorizontalLinearB.X);
            Vector3Wide.Dot(basis.Z, velocityB.Linear, out negatedHorizontalLinearB.Y);
            Matrix2x3Wide.TransformByTransposeWithoutOverlap(velocityB.Angular, horizontalAngularJacobianB, out var horizontalAngularB);
            Vector2Wide.Add(horizontalAngularA, horizontalAngularB, out var horizontalAngular);
            Vector2Wide.Subtract(horizontalLinearA, negatedHorizontalLinearB, out var horizontalLinear);
            Vector2Wide.Add(horizontalAngular, horizontalLinear, out var horizontalVelocity);

            Symmetric3x3Wide.MatrixSandwich(horizontalAngularJacobianA, inertiaA.InverseInertiaTensor, out var horizontalAngularContributionA);
            Symmetric3x3Wide.MatrixSandwich(horizontalAngularJacobianB, inertiaB.InverseInertiaTensor, out var horizontalAngularContributionB);
            Symmetric2x2Wide.Add(horizontalAngularContributionA, horizontalAngularContributionB, out var inverseHorizontalEffectiveMass);

            var linearContribution = inertiaA.InverseMass + inertiaB.InverseMass;
            inverseHorizontalEffectiveMass.XX += linearContribution;
            inverseHorizontalEffectiveMass.YY += linearContribution;
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

            ApplyHorizontalImpulse(basis, horizontalAngularJacobianA, horizontalAngularJacobianB, horizontalCorrectiveImpulse, inertiaA, inertiaB, ref velocityA, ref velocityB);

            Vector3Wide.Dot(basis.Y, velocityA.Linear, out var verticalLinearA);
            Vector3Wide.Dot(velocityA.Angular, verticalAngularJacobianA, out var verticalAngularA);
            Vector3Wide.Dot(basis.Y, velocityB.Linear, out var negatedVerticalLinearB);
            Vector3Wide.Dot(velocityB.Angular, verticalAngularJacobianB, out var verticalAngularB);

            var verticalBiasVelocity = Vector.Max(Vector<float>.Zero, prestep.Depth * inverseDt);

            Symmetric3x3Wide.VectorSandwich(verticalAngularJacobianA, inertiaA.InverseInertiaTensor, out var verticalAngularContributionA);
            Symmetric3x3Wide.VectorSandwich(verticalAngularJacobianB, inertiaB.InverseInertiaTensor, out var verticalAngularContributionB);
            var inverseVerticalEffectiveMass = verticalAngularContributionA + verticalAngularContributionB + linearContribution;
            var verticalCorrectiveImpulse = (verticalBiasVelocity - verticalLinearA + negatedVerticalLinearB - verticalAngularA - verticalAngularB) / inverseVerticalEffectiveMass;

            var previousVerticalAccumulatedImpulse = accumulatedImpulses.Vertical;
            var maximumVerticalImpulse = prestep.MaximumVerticalForce * dtWide;
            accumulatedImpulses.Vertical = Vector.Min(Vector<float>.Zero, Vector.Max(accumulatedImpulses.Vertical + verticalCorrectiveImpulse, -maximumVerticalImpulse));
            verticalCorrectiveImpulse = accumulatedImpulses.Vertical - previousVerticalAccumulatedImpulse;

            ApplyVerticalImpulse(basis, verticalAngularJacobianA, verticalAngularJacobianB, verticalCorrectiveImpulse, inertiaA, inertiaB, ref velocityA, ref velocityB);
        }


        public bool RequiresIncrementalSubstepUpdates => true;
        public void IncrementallyUpdateForSubstep(in Vector<float> dt, in BodyVelocityWide velocityA, in BodyVelocityWide velocityB, ref DynamicCharacterMotionPrestep prestep)
        {
            Vector3Wide.CrossWithoutOverlap(velocityA.Angular, prestep.OffsetFromCharacter, out var wxra);
            Vector3Wide.Add(wxra, velocityA.Linear, out var contactVelocityA);

            var normal = QuaternionWide.TransformUnitY(prestep.SurfaceBasis);
            Vector3Wide.CrossWithoutOverlap(velocityB.Angular, prestep.OffsetFromSupport, out var wxrb);
            Vector3Wide.Add(wxrb, velocityB.Linear, out var contactVelocityB);
            Vector3Wide.Subtract(contactVelocityA, contactVelocityB, out var contactVelocityDifference);
            Vector3Wide.Dot(normal, contactVelocityDifference, out var estimatedDepthChangeVelocity);
            prestep.Depth -= estimatedDepthChangeVelocity * dt;
        }
    }
}
