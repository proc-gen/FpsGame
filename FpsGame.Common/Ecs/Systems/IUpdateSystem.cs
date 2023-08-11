using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Ecs.Systems
{
    public interface IUpdateSystem
    {
        void Update(GameTime gameTime, float totalElapsedTime, float scaleFactor);
    }
}
