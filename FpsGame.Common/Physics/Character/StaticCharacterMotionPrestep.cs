using BepuUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public struct StaticCharacterMotionPrestep
    {
        public QuaternionWide SurfaceBasis;
        public Vector<float> MaximumHorizontalForce;
        public Vector<float> MaximumVerticalForce;
        public Vector<float> Depth;
        public Vector2Wide TargetVelocity;
        public Vector3Wide OffsetFromCharacter;
    }
}
