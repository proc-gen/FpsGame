using Arch.Core;
using FpsGame.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Ecs.Systems
{
    public abstract class ArchSystem
    {
        protected World world;
        protected Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        protected ArchSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
        {
            this.world = world;
            this.queryDescriptions = queryDescriptions;
        }
    }
}
