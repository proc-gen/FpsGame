using Arch.Core;
using BepuPhysics;
using BepuPhysics.Collidables;
using FpsGame.Common.Components;
using FpsGame.Common.Physics;
using FpsGame.Common.Serialization.ComponentConverters;
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

        private void createFloorTile(Vector3 position)
        {
            world.Create(
                new RenderModel() { Model = "floor-tile" },
                new Position() { X = position.X, Y = position.Y, Z = position.Z },
                new Rotation() { X = -MathF.PI / 2f },
                new Scale(5),
                physicsWorld.AddStatic(new StaticDescription(position, physicsWorld.Simulation.Shapes.Add(new Box(20, 2, 20)))),
                new FullSerializeOnly()
            );
        }

        private void createWall(Vector3 position, float yRot)
        {
            Vector3 positionOffset = new Vector3(MathF.Sin(yRot), 0, MathF.Cos(yRot));

            world.Create(
                new RenderModel() { Model = "metal-wall" },
                new Position() { X = position.X, Y = position.Y + 0.5f, Z = position.Z },
                new Rotation() { Y = yRot },
                new Scale(2),
                physicsWorld.AddStatic(new StaticDescription(new RigidPose(position, Quaternion.CreateFromYawPitchRoll(yRot, 0, 0)), physicsWorld.Simulation.Shapes.Add(new Box(10, 20, 2)))),
                new FullSerializeOnly()
            );

            world.Create(
                new RenderModel() { Model = "metal-wall" },
                new Position() { X = position.X + positionOffset.X, Y = position.Y + 2 * 1.867f + 0.5f, Z = position.Z + positionOffset.Z },
                new Rotation() { X = MathF.PI / 6f, Y = yRot },
                new Scale(2),
                new FullSerializeOnly()
            );

            world.Create(
                new RenderModel() { Model = "metal-wall" },
                new Position() { X = position.X + positionOffset.X, Y = position.Y - 2 * 1.867f + 0.5f, Z = position.Z + positionOffset.Z },
                new Rotation() { X = -MathF.PI / 6f, Y = yRot },
                new Scale(2),
                new FullSerializeOnly()
            );
        }

        public override void PopulateLevel()
        {
            //Floor
            for(int i = -5; i <= 5; i++)
            {
                for (int j = -5; j <= 5; j++)
                {
                    createFloorTile(new Vector3(i * 10, -5, j * 10));
                }
            }

            //Wall X+
            for(int i = -12; i <= 12; i++)
            {
                createWall(new Vector3(48.5f, 0, i * 4), -MathF.PI / 2f);
            }

            //Wall X-
            for (int i = -12; i <= 12; i++)
            {
                createWall(new Vector3(-48.5f, 0, i * 4), MathF.PI / 2f);
            }

            //Wall Z+
            for (int i = -12; i <= 12; i++)
            {
                createWall(new Vector3(i * 4, 0, 48.5f), MathF.PI);
            }

            //Wall Z-
            for (int i = -12; i <= 12; i++)
            {
                createWall(new Vector3(i * 4, 0, -48.5f), 0);
            }
        }
    }
}
