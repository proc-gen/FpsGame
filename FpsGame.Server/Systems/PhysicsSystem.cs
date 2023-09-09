﻿using Arch.Core;
using BepuPhysics;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Physics;
using FpsGame.Common.Physics.Character;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.Systems
{
    public class PhysicsSystem : ArchSystem, IUpdateSystem
    {
        protected PhysicsWorld physicsWorld;
        public PhysicsSystem(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions, PhysicsWorld physicsWorld) 
            : base(world, queryDescriptions)
        {
            this.physicsWorld = physicsWorld;
        }

        public void Update(GameTime gameTime)
        {
            physicsWorld.Step();

            var query = queryDescriptions[QueryDescriptions.CharacterPhysicsBodies];
            world.Query(in query, (ref Camera camera, ref CharacterInput body) =>
            {
                var newPosition = physicsWorld.Simulation.Bodies[body.Body].Pose.Position;
                camera.Position = newPosition;
                camera.ComponentState = SerializableObjectState.Update;
            });

            var query2 = queryDescriptions[QueryDescriptions.DynamicPhysicsBodies];
            world.Query(in query2, (ref Position position, ref Rotation rotation, ref CharacterInput body) =>
            {
                var pose = physicsWorld.Simulation.Bodies[body.Body].Pose;

                position.X = pose.Position.X;
                position.Y = pose.Position.Y;
                position.Z = pose.Position.Z;

                var q = pose.Orientation;

                rotation.X = MathF.Atan2(2.0f * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.W * q.W - q.W * q.W + q.W * q.W);
                rotation.Z = MathF.Asin(-2.0f * (q.W * q.W - q.W * q.W));
                rotation.Y = MathF.Atan2(2.0f * (q.W * q.W + q.W * q.W), q.W * q.W + q.W * q.W - q.W * q.W + q.W * q.W - q.W * q.W + q.W * q.W);

                position.ComponentState = SerializableObjectState.Update;
                rotation.ComponentState = SerializableObjectState.Update;
            });
        }
    }
}
