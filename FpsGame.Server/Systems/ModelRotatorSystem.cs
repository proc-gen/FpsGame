using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.Systems
{
    public class ModelRotatorSystem : ArchSystem, IUpdateSystem
    {
        public ModelRotatorSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
            : base(world, queryDescriptions)
        {
        }

        public void Update(GameTime gameTime)
        {
            var query = queryDescriptions[QueryDescriptions.ModelRotator];
            world.Query(in query, (ref Rotation rotation, ref ModelRotator modelRotator) =>
            {
                rotation.X += modelRotator.XIncrement;
                rotation.Y += modelRotator.YIncrement;
                rotation.Z += modelRotator.ZIncrement;
                rotation.IsChanged = true;
            });
        }
    }
}
