using FpsGame.Common.ClientData;
using FpsGame.Common.Constants;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using FpsGame.Ui.Styles;
using FpsGame.UiComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FpsGame.Screens
{
    public class ServerScreen : Screen, IDisposable
    {
        private bool disposedValue = false;
        Server.Server server;
        CancellationTokenSource token = new CancellationTokenSource();
        private GameSettings gameSettings;

        Label hostLocationLabel;
        Label gameNameLabel;
        VerticalPanel gameInfoPanel;

        VerticalPanel messagesPanel;
        ChatBox chatBox;

        PlayersTable playersTable;

        Panel hudPanel;

        public ServerScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            hostLocationLabel = new Label("host-location", gameSettings.GameIPAddress.ToString() + ":" + gameSettings.GamePort, new Style()
            {
                Margin = new Thickness(0),
            });
            gameNameLabel = new Label("game-name", gameSettings.GameName, new Style()
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

            server = new Server.Server(token.Token, gameSettings, GetChatMessages);

        }

        private bool GetChatMessages(List<ChatMessage> messages)
        {
            foreach (ChatMessage message in messages)
            {
                chatBox.AddMessage(message);
            }

            return true;
        }

        public override void Update(GameTime gameTime)
        {
            var gState = GamePad.GetState(PlayerIndex.One);
            var kState = Keyboard.GetState();
            var mState = Mouse.GetState();

            if (gState.Buttons.Back == ButtonState.Pressed
                || kState.IsKeyDown(Keys.Escape))
            {
                token.Cancel();
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

            server?.Run(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    server.Dispose();
                    server = null;
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
