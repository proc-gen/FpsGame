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

        public void SetActive(Desktop desktop)
        {
            desktop.Root = RootWidget;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

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
