using BepuPhysics.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public class StaticCharacterMotionTypeProcessor : OneBodyTypeProcessor<StaticCharacterMotionPrestep, CharacterMotionAccumulatedImpulse, StaticCharacterMotionFunctions, AccessAll, AccessAll>
    {
        public const int BatchTypeId = 50;
    }
}
