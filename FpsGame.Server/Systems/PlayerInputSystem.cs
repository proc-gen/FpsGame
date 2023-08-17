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
        float sensitivity = 0.02f;
        public PlayerInputSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
            : base(world, queryDescriptions)
        {
        }

        public void Update(GameTime gameTime)
        {
            var query = queryDescriptions[QueryDescriptions.PlayerInput];
            world.Query(in query, (ref Camera camera, ref ClientInput clientInput) =>
            {
                camera.IsChanged = clientInput.Direction != Vector3.Zero || clientInput.MouseDelta != Vector2.Zero;

                if (camera.IsChanged)
                {
                    camera.Position += clientInput.Direction / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    camera.Yaw += clientInput.MouseDelta.X * sensitivity;
                    camera.Pitch += -1f * clientInput.MouseDelta.Y * sensitivity;
                }
            });
        }
    }
}
