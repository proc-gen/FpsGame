using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Containers;
using FpsGame.Common.Ecs;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server;
using FpsGame.Server.ClientData;
using FpsGame.Systems;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using FpsGame.Ui.Styles;
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
        SerializableWorld serializableWorld = new SerializableWorld(false);
        List<IRenderSystem> renderSystems;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        Server.Server server;
        Client client;
        CancellationTokenSource token = new CancellationTokenSource();
        Queue<string> ServerData = new Queue<string>();
        private readonly JsonNetSerializer serializer = new JsonNetSerializer();
        private readonly Dictionary<Type, Converter> converters;
        
        private Vector2 lastMousePosition;
        private bool firstMove = true;

        private GameSettings gameSettings;
        private uint PlayerId;
        private EntityReference Player = EntityReference.Null;

        Label hostLocationLabel;
        Label gameNameLabel;
        VerticalPanel gameInfoPanel;

        List<Task> tasks = new List<Task>();

        public GameScreen(Game game, ScreenManager screenManager, GameSettings gameSettings, PlayerSettings? playerSettings)
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;
            Models.Add("cube", game.Content.Load<Model>("cube"));
            Models.Add("sphere", game.Content.Load<Model>("sphere"));

            world = World.Create();

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.RenderModel, new QueryDescription().WithAll<RenderModel, Position, Rotation, Scale>() },
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Camera>() },
                { QueryDescriptions.RenderPlayer, new QueryDescription().WithAll<Player, RenderModel, Camera>() }
            };

            renderSystems = new List<IRenderSystem>()
            {
                new RenderModelSystem(world, queryDescriptions, Models),
                new RenderPlayerSystem(world, queryDescriptions, Models),
            };

            converters = new Dictionary<Type, Converter>()
            {
                {typeof(RenderModel), new RenderModelConverter()},
                {typeof(Position), new PositionConverter()},
                {typeof(Rotation), new RotationConverter()},
                {typeof(Scale), new ScaleConverter()},
                {typeof(ModelRotator), new ModelRotatorConverter()},
                {typeof(Player), new PlayerConverter() },
                {typeof(Camera), new CameraConverter() },
            };

            if (gameSettings.GameMode != GameMode.MultiplayerJoin)
            {
                server = new Server.Server(token.Token, gameSettings);
            }

            if (gameSettings.GameMode != GameMode.StandaloneServer)
            {
                client = new Client(AddDataToProcess, gameSettings, playerSettings.Value);
                tasks.Add(Task.Run(() => client.Join(token.Token), token.Token));

                hostLocationLabel = new Label("host-location", gameSettings.GameIPAddress.First().ToString() + ":" + gameSettings.GamePort);
                gameNameLabel = new Label("game-name", gameSettings.GameName);
                gameInfoPanel = new VerticalPanel("game-info", new Style()
                {
                    Margin = new Thickness(4),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                });
                gameInfoPanel.AddWidget(gameNameLabel);
                gameInfoPanel.AddWidget(hostLocationLabel);

                RootWidget = gameInfoPanel.UiWidget;
            }
        }

        public override void Update(GameTime gameTime)
        {
            var gState = GamePad.GetState(PlayerIndex.One);
            var kState = Keyboard.GetState();
            var mState = Mouse.GetState();

            if (gState.Buttons.Back == ButtonState.Pressed 
                || kState.IsKeyDown(Keys.Escape))
            {
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

            if (client != null)
            {
                processServerData();
                processInputData(kState, mState, gState);
            }

            server?.Run(gameTime);
        }

        private void processServerData()
        {
            if (ServerData.Count > 0)
            {
                var data = ServerData.Dequeue();
                serializer.Deserialize(data, serializableWorld);

                if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Add).Any())
                {
                    foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Add))
                    {
                        Entity created = world.CreateFromArray(entity.GetDeserializedComponents(converters));
                        entity.EntityReference = created.Reference();
                        entity.DestinationId = created.Id;
                        entity.DestinationVersionId = created.Version();
                        entity.EntityState = SerializableObjectState.NoChange;
                    }
                }

                if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Update).Any())
                {
                    foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Update))
                    {
                        world.SetFromArray(entity.EntityReference.Entity, entity.GetDeserializedComponents(converters));
                        entity.EntityState = SerializableObjectState.NoChange;
                    }
                }

                if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Remove).Any())
                {
                    foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Remove))
                    {
                        world.Destroy(entity.EntityReference);
                        entity.EntityReference = EntityReference.Null;
                    }

                    serializableWorld.Entities.RemoveAll(a => a.EntityState == SerializableObjectState.Remove);
                }

                if (serializableWorld.EntitiesToRemove.Any())
                {
                    foreach (var entity in serializableWorld.EntitiesToRemove)
                    {
                        world.Destroy(entity.EntityReference);
                        entity.EntityReference = EntityReference.Null;
                    }

                    serializableWorld.EntitiesToRemove.Clear();
                }

                if (serializableWorld.MessageType == MessageType.WorldFull)
                {
                    PlayerId = serializableWorld.PlayerId;

                    var playerQuery = queryDescriptions[QueryDescriptions.PlayerInput];

                    world.Query(in playerQuery, (in Entity entity, ref Player player) =>
                    {
                        if (player.Id == PlayerId)
                        {
                            Player = entity.Reference();
                        }
                    });
                }
            }
        }

        private void processInputData(KeyboardState kState, MouseState mState, GamePadState gState)
        {
            if (Game.IsActive)
            {
                var keys = kState.GetPressedKeys();

                ClientInput clientInput = new ClientInput()
                {
                    Forward = keys.Contains(Keys.Up) || keys.Contains(Keys.W) || gState.DPad.Up == ButtonState.Pressed,
                    Backward = keys.Contains(Keys.Down) || keys.Contains(Keys.S) || gState.DPad.Down == ButtonState.Pressed,
                    Left = keys.Contains(Keys.Left) || keys.Contains(Keys.A) || gState.DPad.Left == ButtonState.Pressed,
                    Right = keys.Contains(Keys.Right) || keys.Contains(Keys.D) || gState.DPad.Right == ButtonState.Pressed,
                    LeftStick = gState.ThumbSticks.Left,
                    RightStick = gState.ThumbSticks.Right,
                    
                };

                if (mState.RightButton == ButtonState.Pressed)
                {
                    if (firstMove)
                    {
                        firstMove = false;
                    }
                    else if (!firstMove)
                    {
                        clientInput.MouseDelta = mState.Position.ToVector2() - lastMousePosition;
                    }
                    lastMousePosition = mState.Position.ToVector2();
                }

                if (clientInput.Forward ||
                    clientInput.Backward ||
                    clientInput.Left ||
                    clientInput.Right ||
                    clientInput.MouseDelta != Vector2.Zero ||
                    clientInput.LeftStick != Vector2.Zero ||
                    clientInput.RightStick != Vector2.Zero)
                {
                    client.SendInputData(clientInput);
                }
            }
        }

        public override void Render(GameTime gameTime)
        {
            if(gameSettings.GameMode != GameMode.StandaloneServer)
            {
                renderGame(gameTime);
            }
            else
            {
                renderServer(gameTime);
            }
        }

        private void renderGame(GameTime gameTime)
        {
            if (Player != EntityReference.Null)
            {
                Entity player = Player.Entity;
                Camera playerCamera = player.Get<Camera>();

                view = playerCamera.GetViewMatrix();
                projection = playerCamera.GetProjectionMatrix();

                foreach (var system in renderSystems)
                {
                    system.Render(gameTime, view, projection);
                }
            }
        }

        private void renderServer(GameTime gameTime)
        {

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
                    token?.Cancel();
                    server?.Dispose();
                    client?.Dispose();
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
