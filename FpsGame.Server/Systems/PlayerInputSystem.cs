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
        float sensitivity = 0.2f;
        public PlayerInputSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
            : base(world, queryDescriptions)
        {
        }

        public void Update(GameTime gameTime)
        {
            var query = queryDescriptions[QueryDescriptions.PlayerInput];
            world.Query(in query, (ref Camera camera, ref ClientInput clientInput) =>
            {
                camera.IsChanged =
                    clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right || 
                    clientInput.MouseDelta != Vector2.Zero;

                if (camera.IsChanged)
                {
                    if(clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right)
                    {
                        Vector3 movement = Vector3.Zero;

                        if (clientInput.Forward)
                        {
                            movement += camera.Front;
                        }
                        if (clientInput.Backward)
                        {
                            movement -= camera.Front;
                        }
                        if (clientInput.Left)
                        {
                            movement -= camera.Right;
                        }
                        if (clientInput.Right)
                        {
                            movement += camera.Right;
                        }

                        camera.Position += Vector3.Normalize(movement) / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    if(clientInput.MouseDelta != Vector2.Zero)
                    {
                        camera.YawAsDegrees += clientInput.MouseDelta.X * sensitivity;
                        camera.PitchAsDegrees += -1f * clientInput.MouseDelta.Y * sensitivity;
                    }

                    

                    clientInput.Forward = clientInput.Backward = clientInput.Left = clientInput.Right = false;
                    clientInput.MouseDelta = Vector2.Zero;
                }
            });
        }
    }
}
