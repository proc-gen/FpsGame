using BepuPhysics.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics.Character
{
    public class DynamicCharacterMotionTypeProcessor : TwoBodyTypeProcessor<DynamicCharacterMotionPrestep, CharacterMotionAccumulatedImpulse, DynamicCharacterMotionFunctions, AccessAll, AccessAll, AccessAll, AccessAll>
    {
        public const int BatchTypeId = 51;
    }
}
