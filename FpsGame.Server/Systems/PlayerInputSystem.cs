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
            world.Query(in query, (ref Position position, ref ClientInput clientInput) =>
            {
                if (clientInput.Direction != Vector3.Zero)
                {
                    position.X += clientInput.Direction.X / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    position.Y += clientInput.Direction.Y / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    position.Z += clientInput.Direction.Z / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    position.IsChanged = true;

                    clientInput.Direction = Vector3.Zero;
                }
            });
        }
    }
}
