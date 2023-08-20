using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FpsGame.Systems
{
    internal class RenderPlayerSystem : ArchSystem, IRenderSystem
    {
        Dictionary<string, Model> models;

        public RenderPlayerSystem(
            World world, 
            Dictionary<QueryDescriptions, QueryDescription> queryDescriptions,
            Dictionary<string, Model> models) 
            : base(world, queryDescriptions)
        {
            this.models = models;
        }

        public void Render(GameTime gameTime, Matrix view, Matrix projection)
        {
            var query = queryDescriptions[QueryDescriptions.RenderPlayer];
            world.Query(in query, (ref RenderModel renderModel, ref Camera camera) => 
            {
                DrawModel(view, projection, models[renderModel.Model], camera);
            });
        }

        private void DrawModel(Matrix view, Matrix projection, Model model, Camera camera)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = Color.Red.ToVector3();
                    effect.World = Matrix.CreateTranslation(camera.Position);
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}
