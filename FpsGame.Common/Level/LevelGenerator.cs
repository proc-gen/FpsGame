using Arch.Core;
using BepuPhysics;
using BepuPhysics.Collidables;
using FpsGame.Common.Components;
using FpsGame.Common.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Level
{
    public class LevelGenerator : Level
    {
        public LevelGenerator(World world, PhysicsWorld physicsWorld)
            :base(world, physicsWorld)
        { 
        }

        public override void PopulateLevel()
        {
            //Floor
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = -5f, Z = 0 },
                new Rotation(),
                new Scale() { X = 50, Y = 1, Z = 50 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(0, -5, 0), physicsWorld.Simulation.Shapes.Add(new Box(50, 1, 50))))
            );

            //Wall X+
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 50.5f, Y = 0, Z = 0 },
                new Rotation(),
                new Scale() { X = 1, Y = 10, Z = 50 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(50.5f, 0, 0), physicsWorld.Simulation.Shapes.Add(new Box(1, 10, 50))))
            
            );

            //Wall X-
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = -50.5f, Y = 0, Z = 0 },
                new Rotation(),
                new Scale() { X = 1, Y = 10, Z = 50 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(-50.5f, 0, 0), physicsWorld.Simulation.Shapes.Add(new Box(1, 10, 50))))

            );

            //Wall Z+
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = 50.5f },
                new Rotation(),
                new Scale() {X = 50, Y = 10, Z = 1 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(0, 0, 50.5f), physicsWorld.Simulation.Shapes.Add(new Box(50, 10, 1))))

            );

            //Wall Z-
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = -50.5f },
                new Rotation(),
                new Scale() { X = 50, Y = 10, Z = 1 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(0, 0, -50.5f), physicsWorld.Simulation.Shapes.Add(new Box(50, 10, 1))))

            );

            //Center
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = 0 },
                new Rotation(),
                new Scale() { X = 10, Y = 10, Z = 10 },
                physicsWorld.AddStatic(new StaticDescription(new Vector3(0, 0, 0), physicsWorld.Simulation.Shapes.Add(new Box(10, 10, 10))))
            
            );
        }
    }
}
