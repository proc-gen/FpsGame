using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Server.ClientData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.Systems
{
    public class PlayerInputSystem : ArchSystem, IUpdateSystem
    {
        public PlayerInputSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
            : base(world, queryDescriptions)
        {
        }

        public void Update(GameTime gameTime)
        {
            var query = queryDescriptions[QueryDescriptions.PlayerInput];
            world.Query(in query, (ref Camera camera, ref ClientInput clientInput) =>
            {
                if (clientInput.Direction != Vector3.Zero)
                {
                    camera.Position += clientInput.Direction / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    camera.IsChanged = true;

                    clientInput.Direction = Vector3.Zero;
                }
            });
        }
    }
}
