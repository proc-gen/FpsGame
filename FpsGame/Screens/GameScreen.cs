using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server;
using FpsGame.Systems;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
        SerializableWorld serializableWorld = new SerializableWorld();
        List<IRenderSystem> renderSystems;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        Server.Server server;
        Client client;
        CancellationTokenSource token = new CancellationTokenSource();
        Queue<string> ServerData = new Queue<string>();
        private readonly JsonNetSerializer serializer = new JsonNetSerializer();
        private readonly Dictionary<Type, Converter> converters;

        public GameScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {
            Models.Add("cube", game.Content.Load<Model>("cube"));

            world = World.Create();

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.RenderModel, new QueryDescription().WithAll<RenderModel, Position, Rotation, Scale>() },
            };

            renderSystems = new List<IRenderSystem>()
            {
                new RenderModelSystem(world, queryDescriptions, Models),
            };

            converters = new Dictionary<Type, Converter>()
            {
                {typeof(RenderModel), new RenderModelConverter()},
                {typeof(Position), new PositionConverter()},
                {typeof(Rotation), new RotationConverter()},
                {typeof(Scale), new ScaleConverter()},
                {typeof(ModelRotator), new ModelRotatorConverter()},
            };

            server = new Server.Server();
            Task.Run(() => server.StartListening(token.Token));

            client = new Client(AddDataToProcess);
            Task.Run(() => client.Join(token.Token));
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

            if(ServerData.Count > 0)
            {
                var data = ServerData.Dequeue();
                serializer.Deserialize(data, serializableWorld);

                foreach(var entity in serializableWorld.Entities.Where(a => a.Create))
                {
                    Entity created = world.CreateFromArray(entity.GetDeserializedComponents(converters));
                    entity.EntityReference = created.Reference();
                    entity.DestinationId = created.Id;
                    entity.DestinationVersionId = created.Version();
                    entity.Create = false;
                }

                foreach(var entity in serializableWorld.Entities.Where(a => a.Update))
                {
                    world.SetFromArray(entity.EntityReference.Entity, entity.GetDeserializedComponents(converters));
                    entity.Update = false;
                }

                foreach (var entity in serializableWorld.Entities.Where(a => a.Delete))
                {
                    world.Destroy(entity.EntityReference);
                    entity.EntityReference = EntityReference.Null;
                    entity.Delete = false;
                }
            }
        }

        public override void Render(GameTime gameTime)
        {
            foreach(var system in renderSystems)
            {
                system.Render(gameTime, view, projection);
            }   
        }

        public bool AddDataToProcess(string worldData)
        {
            ServerData.Enqueue(worldData);
            return true;
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
