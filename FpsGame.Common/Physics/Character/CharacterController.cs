using BepuPhysics.Collidables;
using BepuPhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace FpsGame.Common.Physics.Character
{
    public struct CharacterController
    {
        public Vector3 ViewDirection;
        public Vector2 TargetVelocity;
        public bool TryJump;

        public BodyHandle BodyHandle;
        public Vector3 LocalUp;
        public float JumpVelocity;
        public float MaximumHorizontalForce;
        public float MaximumVerticalForce;
        public float CosMaximumSlope;
        public float MinimumSupportDepth;
        public float MinimumSupportContinuationDepth;

        public bool Supported;
        public CollidableReference Support;
        public ConstraintHandle MotionConstraintHandle;
    }
}
