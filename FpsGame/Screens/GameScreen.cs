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
using FpsGame.Common.Utils;
using FpsGame.MessageProcessors;
using Myra.Graphics2D.Brushes;
using FpsGame.Containers;

namespace FpsGame.Screens
{
    public class GameScreen : Screen, IDisposable
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
        Queue<JObject> ServerData = new Queue<JObject>();

        private Vector2 lastMousePosition;
        private bool firstMove = true;

        private GameSettings gameSettings;
        private PlayerSettings playerSettings;
        private EntityReference Player = EntityReference.Null;
        Dictionary<string, IMessageProcessor> MessageProcessors = new Dictionary<string, IMessageProcessor>();

        Label hostLocationLabel;
        Label gameNameLabel;
        Label playerPositionLabel;
        VerticalPanel gameInfoPanel;

        VerticalPanel messagesPanel;
        ChatBox chatBox;

        PlayersTable playersTable;

        Panel hudPanel;

        List<Task> tasks = new List<Task>();

        AssetImporter importer;

        SettingsContainer settings;

        public GameScreen(Game game, ScreenManager screenManager, GameSettings gameSettings, PlayerSettings playerSettings)
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;
            this.playerSettings = playerSettings;
            settings = ((Game1)Game).settings;

            loadAssets();
            initECS();
            initClientServer();           
            initUIComponents();
            initMessageProcessors();
        }

        private void loadAssets()
        {
            importer = new AssetImporter(Game.GraphicsDevice);

            Models.Add("cube", importer.LoadModel("Content/cube.fbx"));
            Models.Add("sphere", importer.LoadModel("Content/sphere.fbx"));
            Models.Add("capsule", importer.LoadModel("Content/capsule.fbx"));
            Models.Add("floor-tile", importer.LoadModel("Content/floor-tile.fbx"));
            Models.Add("metal-wall", importer.LoadModel("Content/metal-wall.fbx"));
        }

        private void initECS()
        {
            world = World.Create();

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.RenderModel, new QueryDescription().WithAll<RenderModel, Position, Rotation, Scale>() },
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Camera>() },
                { QueryDescriptions.RenderPlayer, new QueryDescription().WithAll<Player, RenderModel, Camera, Position>() }
            };

            renderSystems = new List<IRenderSystem>()
            {
                new RenderModelSystem(world, queryDescriptions, Models),
                new RenderPlayerSystem(world, queryDescriptions, Models),
            };
        }

        private void initClientServer()
        {
            if (gameSettings.GameMode != GameMode.MultiplayerJoin)
            {
                server = new Server.Server(token.Token, gameSettings);
            }

            playerSettings.MouseSensitivity = settings.MouseSensitivity;
            playerSettings.ControllerSensitivity = settings.ControllerSensitivity;
            client = new Client(AddDataToProcess, gameSettings, playerSettings);
            tasks.Add(Task.Run(() => client.Join(token.Token), token.Token));
        }

        private void initUIComponents()
        {
            gameInfoPanel = new VerticalPanel("game-info", new Style()
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            });

            if (gameSettings.GameMode != GameMode.SinglePlayer)
            {
                hostLocationLabel = new Label("host-location", gameSettings.GameIPAddress.ToString() + ":" + gameSettings.GamePort, new Style()
                {
                    Margin = new Thickness(0),
                });
                gameNameLabel = new Label("game-name", gameSettings.GameName, new Style()
                {
                    Margin = new Thickness(0),
                });
                gameInfoPanel.AddWidget(gameNameLabel);
                gameInfoPanel.AddWidget(hostLocationLabel);
            }
            
            playerPositionLabel = new Label("player-position", string.Empty, new Style()
            {
                Margin = new Thickness(0),
            });
            
            gameInfoPanel.AddWidget(playerPositionLabel);
            gameInfoPanel.UiWidget.Background = new SolidBrush(new Color(.1f, .1f, .1f));
            gameInfoPanel.UiWidget.Opacity = .75f;

            chatBox = new ChatBox();
            messagesPanel = new VerticalPanel("messages-panel", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
            });
            messagesPanel.AddWidget(chatBox.MessagesLabel);
            messagesPanel.UiWidget.Background = new SolidBrush(new Color(.1f, .1f, .1f));
            messagesPanel.UiWidget.Opacity = .75f;

            playersTable = new PlayersTable();

            hudPanel = new Panel("hud-panel");
            hudPanel.AddWidget(gameInfoPanel);
            hudPanel.AddWidget(messagesPanel);
            hudPanel.AddWidget(playersTable.Table);

            RootWidget = hudPanel.UiWidget;
        }

        private void initMessageProcessors()
        {
            MessageProcessors.Add("SerializableWorld", new SerializableWorldProcessor(world, queryDescriptions));
            MessageProcessors.Add("ChatMessage", new ChatMessageProcessor(chatBox));
            MessageProcessors.Add("GameSettings", new GameSettingsProcessor(gameSettings, gameNameLabel));
            MessageProcessors.Add("PlayersInfo", new PlayersInfoProcessor(playersTable));
        }

        public override void Update(GameTime gameTime)
        {
            var gState = GamePad.GetState(PlayerIndex.One);
            var kState = Keyboard.GetState();
            var mState = Mouse.GetState();

            processInputDataLocal(kState, mState, gState);
            processServerData();
            processInputDataToSend(kState, mState, gState);
            updateHudData();

            server?.Update(gameTime);
        }

        private void processInputDataLocal(KeyboardState kState, MouseState mState, GamePadState gState)
        {
            if (gState.Buttons.Back == ButtonState.Pressed
                || kState.IsKeyDown(Keys.Escape))
            {

                client.SendInputData(new ClientDisconnect());
                Thread.Sleep(250);

                token.Cancel();
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

            playersTable.Table.UiWidget.Visible = kState.IsKeyDown(Keys.Tab);
        }

        private void processServerData()
        {
            if (ServerData.Count > 0)
            {
                do
                {
                    var data = ServerData.Dequeue();
                    if (data != null)
                    {
                        MessageProcessors[data["Type"].ToString()].ProcessMessage(data);

                        if (Player == EntityReference.Null && data["Type"].ToString() == "SerializableWorld")
                        {
                            Player = ((SerializableWorldProcessor)MessageProcessors["SerializableWorld"]).Player;
                        }
                    }
                } while(ServerData.Count > 0);
            }
        }

        private void processInputDataToSend(KeyboardState kState, MouseState mState, GamePadState gState)
        {
            if (Game.IsActive)
            {
                var keys = kState.GetPressedKeys();

                ClientInput clientInput = new ClientInput()
                {
                    Forward = keys.Contains(Keys.W),
                    Backward = keys.Contains(Keys.S),
                    Left = keys.Contains(Keys.A),
                    Right = keys.Contains(Keys.D),
                    LeftStick = gState.ThumbSticks.Left,
                    RightStick = gState.ThumbSticks.Right,
                    Jump = keys.Contains(Keys.Space) || gState.Buttons.A == ButtonState.Pressed,
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
                    clientInput.RightStick != Vector2.Zero ||
                    clientInput.Jump)
                {
                    client.SendInputData(clientInput);
                }
            }
        }

        private void updateHudData()
        {
            if (Player != EntityReference.Null)
            {
                var position = Player.Entity.Get<Camera>().Position;
                playerPositionLabel.UpdateText($"( {position.X:0.##}, {position.Y:0.##}, {position.Z:0.##} )");
            }
        }

        public override void Render(GameTime gameTime)
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
