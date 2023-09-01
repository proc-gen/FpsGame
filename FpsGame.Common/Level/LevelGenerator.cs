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
            Random random = new Random();

            int max = 50, halfMax = 25;
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    world.Create(
                        new RenderModel() { Model = "cube" },
                        new Position() { X = i - halfMax, Y = -5f, Z = j - halfMax },
                        new Rotation(),
                        new Scale(0.5f + (float)random.NextDouble())
                    );
                }
            }
        }
    }
}
