using FpsGame.Common.ClientData;
using FpsGame.Common.Constants;
using FpsGame.Ui;
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

        public ServerScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;
            server = new Server.Server(token.Token, gameSettings);
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
