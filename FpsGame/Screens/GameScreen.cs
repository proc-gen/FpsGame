using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Systems;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FpsGame.Screens
{
    public class GameScreen : Screen
    {
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100f);

        World world;
        List<IRenderSystem> renderSystems;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        public GameScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {
            Models.Add("cube", game.Content.Load<Model>("cube"));

            world = World.Create();

            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 2, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = -2, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = 2, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = -2, Z = 0 }, new Rotation(), new Scale(0.5f));

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.RenderModel, new QueryDescription().WithAll<RenderModel, Position, Rotation, Scale>() },
            };

            renderSystems = new List<IRenderSystem>()
            {
                new RenderModelSystem(world, queryDescriptions, Models),
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }
        }

        public override void Render(GameTime gameTime)
        {
            foreach(var system in renderSystems)
            {
                system.Render(gameTime, view, projection);
            }   
        }
    }
}
