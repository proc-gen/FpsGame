using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.ClientData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using BepuPhysics;
using FpsGame.Common.Physics;
using FpsGame.Common.Physics.Character;
using BepuUtilities;
using System.Diagnostics;

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
            world.Query(in query, (ref Camera camera, ref ClientInput clientInput, ref CharacterInput body) =>
            {
                camera.ComponentState = SerializableObjectState.Update;

                UpdatePlayerGoals(ref clientInput, ref body, ref camera);

                if(clientInput.MouseDelta != Vector2.Zero)
                {
                    camera.YawAsDegrees += clientInput.MouseDelta.X * sensitivity;
                    camera.PitchAsDegrees += -1f * clientInput.MouseDelta.Y * sensitivity;
                }

                if (clientInput.RightStick != Vector2.Zero)
                {
                    camera.YawAsDegrees += clientInput.RightStick.X * controllerSensitivity;
                    camera.PitchAsDegrees += clientInput.RightStick.Y * controllerSensitivity;
                }
                
                clientInput.Forward = clientInput.Backward = clientInput.Left = clientInput.Right = false;
                clientInput.MouseDelta = clientInput.LeftStick = clientInput.RightStick = Vector2.Zero;
                clientInput.Jump = false;
            });
        }

        private void UpdatePlayerGoals(ref ClientInput clientInput, ref CharacterInput characterInput, ref Camera camera)
        {
            ref var character = ref physicsWorld.characters.GetCharacterByBodyHandle(characterInput.Body);
            var movementDirection = GetMovementDirection(ref clientInput);
            UpdatePlayerVelocity(ref character, ref clientInput, ref characterInput, ref camera, movementDirection);
        }

        private Vector2 GetMovementDirection(ref ClientInput clientInput)
        {
            Vector2 movementDirection = default;

            if (clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right)
            {
                if (clientInput.Forward)
                {
                    movementDirection += Vector2.UnitY;
                }
                if (clientInput.Backward)
                {
                    movementDirection -= Vector2.UnitY;
                }
                if (clientInput.Left)
                {
                    movementDirection -= Vector2.UnitX;
                }
                if (clientInput.Right)
                {
                    movementDirection += Vector2.UnitX;
                }
            }

            if (clientInput.LeftStick != Vector2.Zero)
            {
                movementDirection = clientInput.LeftStick.X * Vector2.UnitX + clientInput.LeftStick.Y * Vector2.UnitY;
            }

            var movementDirectionLengthSquared = movementDirection.LengthSquared();
            if (movementDirectionLengthSquared > 0)
            {
                movementDirection /= MathF.Sqrt(movementDirectionLengthSquared);
            }

            return movementDirection;
        }
    
        private void UpdatePlayerVelocity(ref CharacterController character, ref ClientInput clientInput, ref CharacterInput characterInput, ref Camera camera, Vector2 movementDirection)
        {
            character.TryJump = clientInput.Jump;
            var characterBody = new BodyReference(characterInput.Body, physicsWorld.characters.Simulation.Bodies);
            var effectiveSpeed = characterInput.Speed;
            var newTargetVelocity = movementDirection * effectiveSpeed;
            var viewDirection = camera.Front;

            if (!characterBody.Awake &&
                ((character.TryJump && character.Supported) ||
                newTargetVelocity != character.TargetVelocity ||
                (newTargetVelocity != Vector2.Zero && character.ViewDirection != viewDirection)))
            {
                physicsWorld.characters.Simulation.Awakener.AwakenBody(character.BodyHandle);
            }
            character.TargetVelocity = new System.Numerics.Vector2(newTargetVelocity.X, newTargetVelocity.Y);
            character.ViewDirection = new System.Numerics.Vector3(viewDirection.X, viewDirection.Y, viewDirection.Z);

            if (!character.Supported && movementDirection != Vector2.Zero)
            {
                QuaternionEx.Transform(character.LocalUp, characterBody.Pose.Orientation, out var characterUp);
                var characterRight = System.Numerics.Vector3.Cross(character.ViewDirection, characterUp);
                var rightLengthSquared = characterRight.LengthSquared();
                if (rightLengthSquared > 1e-10f)
                {
                    characterRight /= MathF.Sqrt(rightLengthSquared);
                    var characterForward = System.Numerics.Vector3.Cross(characterUp, characterRight);
                    var worldMovementDirection = characterRight * movementDirection.X + characterForward * movementDirection.Y;
                    var currentVelocity = Vector3.Dot(characterBody.Velocity.Linear, worldMovementDirection);

                    const float airControlForceScale = .2f;
                    const float airControlSpeedScale = .2f;
                    var airAccelerationDt = characterBody.LocalInertia.InverseMass * character.MaximumHorizontalForce * airControlForceScale * PhysicsWorld.timeStep;
                    var maximumAirSpeed = effectiveSpeed * airControlSpeedScale;
                    var targetVelocity = MathF.Min(currentVelocity + airAccelerationDt, maximumAirSpeed);

                    var velocityChangeAlongMovementDirection = MathF.Max(0, targetVelocity - currentVelocity);
                    characterBody.Velocity.Linear += worldMovementDirection * velocityChangeAlongMovementDirection;
                    Debug.Assert(characterBody.Awake, "Velocity changes don't automatically update objects; the character should have already been woken up before applying air control.");
                }
            }
        }
    }
}
