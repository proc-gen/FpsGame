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
            world.Query(in query, (ref Player player, ref RenderModel renderModel, ref Camera camera) => 
            {
                DrawModel(view, projection, models[renderModel.Model], player, camera);
            });
        }

        private void DrawModel(Matrix view, Matrix projection, Model model, Player player, Camera camera)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.DiffuseColor = player.Color;
                    effect.World = Matrix.CreateFromYawPitchRoll(camera.Yaw, 0f, 0f) * Matrix.CreateTranslation(camera.Position);
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}
