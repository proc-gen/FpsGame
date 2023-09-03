using Arch.Core;
using FpsGame.Common.Physics;
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
        protected PhysicsWorld physicsWorld;
        public Level(World world, PhysicsWorld physicsWorld)
        {
            this.world = world;
            this.physicsWorld = physicsWorld;
        }

        public abstract void PopulateLevel();
    }
}
