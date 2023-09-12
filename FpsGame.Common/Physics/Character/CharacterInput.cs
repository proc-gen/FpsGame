using BepuPhysics;
using BepuPhysics.Collidables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public struct CharacterInput
    {
        public BodyHandle Body {  get; set; }
        public float Speed { get; set; }
        public IConvexShape Shape { get; set; }
    }
}
