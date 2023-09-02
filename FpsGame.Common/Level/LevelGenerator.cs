using Arch.Core;
using FpsGame.Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Level
{
    public class LevelGenerator : Level
    {
        public LevelGenerator(World world)
            :base(world)
        { 
        }

        public override void PopulateLevel()
        {
            //Floor
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = -5f, Z = 0 },
                new Rotation(),
                new Scale() { X = 50, Y = 1, Z = 50 }
            );

            //Ceiling
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 5f, Z = 0 },
                new Rotation(),
                new Scale() { X = 50, Y = 1, Z = 50 }
            );

            //Wall X+
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 50.5f, Y = 0, Z = 0 },
                new Rotation(),
                new Scale() { X = 1, Y = 10, Z = 50 }
            );

            //Wall X-
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = -50.5f, Y = 0, Z = 0 },
                new Rotation(),
                new Scale() { X = 1, Y = 10, Z = 50 }
            );

            //Wall Z+
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = 50.5f },
                new Rotation(),
                new Scale() {X = 50, Y = 10, Z = 1 }
            );

            //Wall Z-
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = -50.5f },
                new Rotation(),
                new Scale() { X = 50, Y = 10, Z = 1 }
            );

            //Center
            world.Create(
                new RenderModel() { Model = "cube" },
                new Position() { X = 0, Y = 0, Z = 0 },
                new Rotation() { Y = MathF.PI / 2f },
                new Scale() { X = 10, Y = 10, Z = 10 }
            );
        }
    }
}
