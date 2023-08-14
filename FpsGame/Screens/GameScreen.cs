﻿using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server;
using FpsGame.Server.ClientData;
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
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Position, Rotation>() },
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
                {typeof(Player), new PlayerConverter() },
            };

            server = new Server.Server();

            client = new Client(AddDataToProcess);
            Task.Run(() => client.Join(token.Token));
        }

        public override void Update(GameTime gameTime)
        {
            var gState = GamePad.GetState(PlayerIndex.One);
            var kState = Keyboard.GetState();

            if (gState.Buttons.Back == ButtonState.Pressed 
                || kState.IsKeyDown(Keys.Escape))
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

            if(kState.GetPressedKeyCount() > 0)
            {
                var keys = kState.GetPressedKeys();
                
                ClientInput clientInput = new ClientInput();

                if (keys.Contains(Keys.Up) || keys.Contains(Keys.W))
                {
                    clientInput.Direction -= Vector3.UnitZ;
                }
                if (keys.Contains(Keys.Down) || keys.Contains(Keys.S))
                {
                    clientInput.Direction += Vector3.UnitZ;
                }
                if (keys.Contains(Keys.Left) || keys.Contains(Keys.A))
                {
                    clientInput.Direction -= Vector3.UnitX;
                }
                if (keys.Contains(Keys.Right) || keys.Contains(Keys.D))
                {
                    clientInput.Direction += Vector3.UnitX;
                }

                if (clientInput.Direction != Vector3.Zero)
                {
                    client.SendInputData(clientInput);
                }
            }

            server.Run(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            Entity player = Entity.Null;
            Position playerPosition = new Position();
            Rotation playerRotation = new Rotation();
            var playerQuery = queryDescriptions[QueryDescriptions.PlayerInput];

            world.Query(in playerQuery, (in Entity entity, ref Position position, ref Rotation rotation) =>
            {
                player = entity;
                playerPosition = position;
                playerRotation = rotation;
            });

            view = Matrix.CreateLookAt(new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z), new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z) + Vector3.Forward, Vector3.UnitY);

            foreach (var system in renderSystems)
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
