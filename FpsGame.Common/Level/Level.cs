using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Level
{
    public abstract class Level
    {
        protected World world;
        public Level(World world)
        {
            this.world = world;
        }

        public abstract void PopulateLevel();
    }
}
