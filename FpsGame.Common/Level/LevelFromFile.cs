using Arch.Core;
using BepuPhysics;
using BepuPhysics.Collidables;
using FpsGame.Common.Components;
using FpsGame.Common.Physics;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Level
{
    public class LevelFromFile : Level
    {
        private string LevelPath;

        public LevelFromFile(World world, PhysicsWorld physicsWorld, string levelPath) 
            : base(world, physicsWorld)
        {
            LevelPath = levelPath;
        }

        public override void PopulateLevel()
        {
            LevelData levelData = JsonFileManager.LoadFile<LevelData>(LevelPath);

            foreach(var levelObject in levelData.LevelObjects)
            {
                if(levelObject.IsDynamic)
                {
                    CreateDynamicObject(levelObject);
                }
                else
                {
                    CreateStaticObject(levelObject);
                }
            }
        }

        private void CreateStaticObject(LevelObject levelObject)
        {
            switch(levelObject.PhysicsObjectType)
            {
                case PhysicsObjectType.None:
                    CreateStatic(levelObject);
                    break;
                case PhysicsObjectType.Box:
                    var box = new Box(levelObject.PhysicsObjectParams.X,
                                        levelObject.PhysicsObjectParams.Y,
                                        levelObject.PhysicsObjectParams.Z);
                    CreateStatic(levelObject, box);
                    break;
                case PhysicsObjectType.Sphere:
                    var sphere = new Sphere(levelObject.PhysicsObjectParams.X);
                    CreateStatic(levelObject, sphere);
                    break;
                case PhysicsObjectType.Capsule:
                    var capsule = new Capsule(levelObject.PhysicsObjectParams.X,
                                        levelObject.PhysicsObjectParams.Y);
                    CreateStatic(levelObject, capsule);
                    break;
            }
        }

        private void CreateStatic(LevelObject levelObject)
        {
            world.Create(
                new RenderModel() { Model = levelObject.ModelName },
                new Position() { X = levelObject.Position.X + levelObject.PositionOffset.Y, Y = levelObject.Position.X + levelObject.PositionOffset.Y, Z = levelObject.Position.Z + levelObject.PositionOffset.Z },
                new Rotation() { X = levelObject.Rotation.X, Y = levelObject.Rotation.Y, Z = levelObject.Rotation.Z },
                new Scale() { X = levelObject.Scale.X, Y = levelObject.Scale.Y, Z = levelObject.Scale.Z },
                new FullSerializeOnly()
            );
        }

        private void CreateStatic<T>(LevelObject levelObject, T physicsObject)
            where T : unmanaged, IShape
        {
            world.Create(
                new RenderModel() { Model = levelObject.ModelName },
                new Position() { X = levelObject.Position.X + levelObject.PositionOffset.Y, Y = levelObject.Position.X + levelObject.PositionOffset.Y, Z = levelObject.Position.Z + levelObject.PositionOffset.Z },
                new Rotation() { X = levelObject.Rotation.X, Y = levelObject.Rotation.Y, Z = levelObject.Rotation.Z },
                new Scale() { X = levelObject.Scale.X, Y = levelObject.Scale.Y, Z = levelObject.Scale.Z },
                physicsWorld.AddStatic(new StaticDescription(new RigidPose(levelObject.Position, Quaternion.CreateFromYawPitchRoll(levelObject.Rotation.Y, levelObject.Rotation.X, levelObject.Rotation.Z)), physicsWorld.Simulation.Shapes.Add(physicsObject))),
                new FullSerializeOnly()
            );
        }

        private void CreateDynamicObject(LevelObject levelObject)
        {
            switch (levelObject.PhysicsObjectType)
            {
                case PhysicsObjectType.None:
                    CreateDynamic(levelObject);
                    break;
                case PhysicsObjectType.Box:
                    var box = new Box(levelObject.PhysicsObjectParams.X,
                                        levelObject.PhysicsObjectParams.Y,
                                        levelObject.PhysicsObjectParams.Z);
                    CreateDynamic(levelObject, box);
                    break;
                case PhysicsObjectType.Sphere:
                    var sphere = new Sphere(levelObject.PhysicsObjectParams.X);
                    CreateDynamic(levelObject, sphere);
                    break;
                case PhysicsObjectType.Capsule:
                    var capsule = new Capsule(levelObject.PhysicsObjectParams.X,
                                        levelObject.PhysicsObjectParams.Y);
                    CreateDynamic(levelObject, capsule);
                    break;
            }
        }

        private void CreateDynamic(LevelObject levelObject)
        {
            world.Create(
                new RenderModel() { Model = levelObject.ModelName },
                new Position() { X = levelObject.Position.X + levelObject.PositionOffset.Y, Y = levelObject.Position.X + levelObject.PositionOffset.Y, Z = levelObject.Position.Z + levelObject.PositionOffset.Z },
                new Rotation() { X = levelObject.Rotation.X, Y = levelObject.Rotation.Y, Z = levelObject.Rotation.Z },
                new Scale() { X = levelObject.Scale.X, Y = levelObject.Scale.Y, Z = levelObject.Scale.Z }
            );
        }

        private void CreateDynamic<T>(LevelObject levelObject, T physicsObject)
            where T : unmanaged, IConvexShape
        {
            world.Create(
                new RenderModel() { Model = levelObject.ModelName },
                new Position() { X = levelObject.Position.X + levelObject.PositionOffset.Y, Y = levelObject.Position.X + levelObject.PositionOffset.Y, Z = levelObject.Position.Z + levelObject.PositionOffset.Z },
                new Rotation() { X = levelObject.Rotation.X, Y = levelObject.Rotation.Y, Z = levelObject.Rotation.Z },
                new Scale() { X = levelObject.Scale.X, Y = levelObject.Scale.Y, Z = levelObject.Scale.Z },
                physicsWorld.AddMoveableObject(levelObject.Position, physicsObject, levelObject.Mass)
            );
        }

    }
}
