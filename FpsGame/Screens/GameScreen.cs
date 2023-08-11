using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Server;
using FpsGame.Systems;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FpsGame.Screens
{
    public class GameScreen : Screen
    {
        private bool disposedValue = false;
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100f);

        World world;
        List<IRenderSystem> renderSystems;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        Server.Server server;
        Client client;
        CancellationTokenSource token = new CancellationTokenSource();

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

            server = new Server.Server();
            Task.Run(() => server.StartListening(token.Token));

            client = new Client();
            Task.Run(() => client.Join(token.Token));
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

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    server.Dispose();
                    client.Dispose();
                }

                disposedValue = true;
            }

            base.Dispose(true);
        }

        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
