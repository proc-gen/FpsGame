using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.ClientData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepuPhysics;
using FpsGame.Common.Physics;

namespace FpsGame.Server.Systems
{
    public class PlayerInputSystem : ArchSystem, IUpdateSystem
    {
        protected PhysicsWorld physicsWorld;

        float sensitivity = 0.2f;
        float controllerSensitivity = 0.5f;

        public PlayerInputSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions, PhysicsWorld physicsWorld) 
            : base(world, queryDescriptions)
        {
            this.physicsWorld = physicsWorld;
        }

        public void Update(GameTime gameTime)
        {
            var query = queryDescriptions[QueryDescriptions.PlayerInput];
            world.Query(in query, (ref Camera camera, ref ClientInput clientInput, ref BodyHandle body) =>
            {
                camera.ComponentState =
                    (clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right || 
                    clientInput.MouseDelta != Vector2.Zero ||
                    clientInput.LeftStick != Vector2.Zero ||
                    clientInput.RightStick != Vector2.Zero) ? SerializableObjectState.Update : SerializableObjectState.NoChange;

                if (camera.ComponentState == SerializableObjectState.Update)
                {
                    if(clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right)
                    {
                        Vector3 movement = Vector3.Zero;

                        if (clientInput.Forward)
                        {
                            movement += new Vector3(camera.Front.X, 0, camera.Front.Z);
                        }
                        if (clientInput.Backward)
                        {
                            movement -= new Vector3(camera.Front.X, 0, camera.Front.Z);
                        }
                        if (clientInput.Left)
                        {
                            movement -= new Vector3(camera.Right.X, 0, camera.Right.Z);
                        }
                        if (clientInput.Right)
                        {
                            movement += new Vector3(camera.Right.X, 0, camera.Right.Z);
                        }

                        movement.Normalize();
                        physicsWorld.Simulation.Bodies[body].ApplyLinearImpulse(new System.Numerics.Vector3(movement.X, movement.Y, movement.Z) * (1f / physicsWorld.Simulation.Bodies[body].LocalInertia.InverseMass));
                    }

                    if(clientInput.MouseDelta != Vector2.Zero)
                    {
                        camera.YawAsDegrees += clientInput.MouseDelta.X * sensitivity;
                        camera.PitchAsDegrees += -1f * clientInput.MouseDelta.Y * sensitivity;
                    }

                    if(clientInput.LeftStick != Vector2.Zero)
                    {
                        Vector3 movement = clientInput.LeftStick.X * (new Vector3(camera.Right.X, 0, camera.Right.Z)) + clientInput.LeftStick.Y * (new Vector3(camera.Front.X, 0, camera.Front.Z));
                        movement.Normalize();
                        physicsWorld.Simulation.Bodies[body].ApplyLinearImpulse(new System.Numerics.Vector3(movement.X, movement.Y, movement.Z) * (1f /physicsWorld.Simulation.Bodies[body].LocalInertia.InverseMass));
                    }

                    if (clientInput.RightStick != Vector2.Zero)
                    {
                        camera.YawAsDegrees += clientInput.RightStick.X * controllerSensitivity;
                        camera.PitchAsDegrees += clientInput.RightStick.Y * controllerSensitivity;
                    }

                    clientInput.Forward = clientInput.Backward = clientInput.Left = clientInput.Right = false;
                    clientInput.MouseDelta = clientInput.LeftStick = clientInput.RightStick = Vector2.Zero;
                }
            });
        }
    }
}
