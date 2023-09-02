using Arch.Core;
using BepuPhysics;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Physics;
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

            var query = queryDescriptions[QueryDescriptions.DynamicPhysicsBodies];
            world.Query(in query, (ref Camera camera, ref BodyHandle body) =>
            {
                var newPosition = physicsWorld.Simulation.Bodies[body].Pose.Position;
                camera.Position = newPosition;
            });
        }
    }
}
