using FpsGame.Ui.Components;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;

namespace FpsGame.Ui
{
    public abstract class Screen : IDisposable
    {
        private bool disposedValue;
        protected Widget RootWidget;
        protected Game Game;
        protected ScreenManager ScreenManager;

        public Screen(Game game, ScreenManager screenManager)
        {
            Game = game;
            ScreenManager = screenManager;
        }

        public virtual void SetActive()
        {
            ScreenManager.Desktop.Root = RootWidget;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Render(GameTime gameTime);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

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
