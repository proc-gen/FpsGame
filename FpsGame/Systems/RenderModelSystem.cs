using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FpsGame.Systems
{
    internal class RenderModelSystem : ArchSystem, IRenderSystem
    {
        Dictionary<string, Model> models;

        public RenderModelSystem(
            World world, 
            Dictionary<QueryDescriptions, QueryDescription> queryDescriptions,
            Dictionary<string, Model> models) 
            : base(world, queryDescriptions)
        {
            this.models = models;
        }

        public void Render(GameTime gameTime, Matrix view, Matrix projection)
        {
            var query = queryDescriptions[QueryDescriptions.RenderModel];
            world.Query(in query, (ref RenderModel renderModel, ref Position position, ref Rotation rotation, ref Scale scale) => 
            {
                DrawModel(view, projection, models[renderModel.Model], position, rotation, scale);
            });
        }

        private void DrawModel(Matrix view, Matrix projection, Model model, Position position, Rotation rotation, Scale scale)
        {
            foreach (var mesh in model.Meshes)
            {
                var world = Matrix.CreateRotationX(rotation.X)
                            * Matrix.CreateRotationY(rotation.Y)
                            * Matrix.CreateRotationZ(rotation.Z)
                            * Matrix.CreateScale(scale.X, scale.Y, scale.Z)
                            * Matrix.CreateTranslation(position.X, position.Y, position.Z);
                foreach (Effect effect in mesh.Effects)
                {
                    if(effect is BasicEffect basicEffect)
                    {
                        basicEffect.EnableDefaultLighting();
                        basicEffect.PreferPerPixelLighting = true;
                        if (!basicEffect.TextureEnabled)
                        {
                            basicEffect.DiffuseColor = Color.Gray.ToVector3();
                        }
                        basicEffect.Alpha = 1;
                        basicEffect.World = world;
                        basicEffect.View = view;
                        basicEffect.Projection = projection;
                    }
                    else
                    {
                        effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
                    }
                    //effect.EnableDefaultLighting();
                    
                    
                }

                mesh.Draw();
            }
        }
    }
}
