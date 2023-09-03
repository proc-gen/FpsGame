using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Common.ClientData;
using FpsGame.Server;
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
using Newtonsoft.Json.Linq;
using FpsGame.UiComponents;
using System.Net.NetworkInformation;
using FpsGame.Common.Utils;

namespace FpsGame.Screens
{
    public class GameScreen : Screen, IDisposable
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
        Queue<JObject> ServerData = new Queue<JObject>();
        private readonly JsonNetArchSerializer serializer = new JsonNetArchSerializer();
        private readonly Dictionary<Type, Converter> converters;

        private Vector2 lastMousePosition;
        private bool firstMove = true;

        private GameSettings gameSettings;
        private uint PlayerId;
        private EntityReference Player = EntityReference.Null;

        Label hostLocationLabel;
        Label gameNameLabel;
        Label playerPositionLabel;
        VerticalPanel gameInfoPanel;

        VerticalPanel messagesPanel;
        ChatBox chatBox;

        PlayersTable playersTable;

        Panel hudPanel;

        List<Task> tasks = new List<Task>();
        List<ChatMessage> chatMessages = new List<ChatMessage>();

        AssetImporter importer;

        public GameScreen(Game game, ScreenManager screenManager, GameSettings gameSettings, PlayerSettings? playerSettings)
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;
            importer = new AssetImporter(game.GraphicsDevice);

            Models.Add("cube", importer.LoadModel("Content/cube.fbx"));
            Models.Add("sphere", importer.LoadModel("Content/sphere.fbx"));
            Models.Add("capsule", importer.LoadModel("Content/capsule.fbx"));

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

            client = new Client(AddDataToProcess, gameSettings, playerSettings.Value);
            tasks.Add(Task.Run(() => client.Join(token.Token), token.Token));

            hostLocationLabel = new Label("host-location", gameSettings.GameIPAddress.ToString() + ":" + gameSettings.GamePort, new Style()
            {
                Margin = new Thickness(0),
            });
            gameNameLabel = new Label("game-name", gameSettings.GameName, new Style()
            {
                Margin = new Thickness(0),
            });
            playerPositionLabel = new Label("player-position", string.Empty, new Style()
            {
                Margin = new Thickness(0),
            });
            gameInfoPanel = new VerticalPanel("game-info", new Style()
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            });
            gameInfoPanel.AddWidget(gameNameLabel);
            gameInfoPanel.AddWidget(hostLocationLabel);
            gameInfoPanel.AddWidget(playerPositionLabel);

            chatBox = new ChatBox();
            messagesPanel = new VerticalPanel("messages-panel", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
            });
            messagesPanel.AddWidget(chatBox.MessagesLabel);
            playersTable = new PlayersTable();

            hudPanel = new Panel("hud-panel");
            hudPanel.AddWidget(gameInfoPanel);
            hudPanel.AddWidget(messagesPanel);
            hudPanel.AddWidget(playersTable.Table);

            RootWidget = hudPanel.UiWidget;
            
        }

        public override void Update(GameTime gameTime)
        {
            var gState = GamePad.GetState(PlayerIndex.One);
            var kState = Keyboard.GetState();
            var mState = Mouse.GetState();

            if (gState.Buttons.Back == ButtonState.Pressed 
                || kState.IsKeyDown(Keys.Escape))
            {
                if (client != null)
                {
                    client.SendInputData(new ClientDisconnect());
                    Thread.Sleep(250);
                }
                token.Cancel();
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

            if (client != null)
            {
                playersTable.Table.UiWidget.Visible = kState.IsKeyDown(Keys.Tab);
                processServerData();
                processInputData(kState, mState, gState);
                if(Player != EntityReference.Null)
                {
                    playerPositionLabel.UpdateText(Player.Entity.Get<Camera>().Position.ToString());
                }
            }

            server?.Run(gameTime);
        }

        private void processServerData()
        {
            if (ServerData.Count > 0)
            {
                do
                {
                    var data = ServerData.Dequeue();

                    switch (data["Type"].ToString())
                    {
                        case "SerializableWorld":
                            processSerializedWorldData(data);
                            break;
                        case "GameSettings":
                            gameSettings.GameName = data["GameName"].ToString();
                            gameNameLabel.UpdateText(gameSettings.GameName);
                            break;
                        case "ChatMessage":
                            var message = data.ToObject<ChatMessage>();
                            chatMessages.Add(message);
                            chatBox.AddMessage(message);
                            break;
                        case "PlayersInfo":
                            playersTable.Update(data.ToObject<PlayersInfo>().Players);
                            break;
                    }
                } while(ServerData.Count > 0);
            }
        }

        private void processSerializedWorldData(JObject data)
        {
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
                    serializableWorld.Entities.RemoveAll(a => a.EntityReference == entity.EntityReference);
                    world.Destroy(entity.EntityReference);
                    entity.EntityReference = EntityReference.Null;
                }

                serializableWorld.EntitiesToRemove.Clear();
            }

            if (serializableWorld.FullLoad)
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
                else if (!firstMove)
                {
                    firstMove = true;
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
            renderGame(gameTime);
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

        public bool AddDataToProcess(JObject data)
        {
            ServerData.Enqueue(data);
            return true;
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (tasks.Any())
                    {
                        foreach (var task in tasks)
                        {
                            if (task.IsCompleted)
                            {
                                task.Dispose();
                            }
                        }
                    }

                    server?.Dispose();
                    server = null;
                    client?.Dispose();
                    client = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
