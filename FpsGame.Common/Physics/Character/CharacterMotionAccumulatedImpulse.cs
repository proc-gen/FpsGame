using BepuUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public struct CharacterMotionAccumulatedImpulse
    {
        public Vector2Wide Horizontal;
        public Vector<float> Vertical;
    }
}
