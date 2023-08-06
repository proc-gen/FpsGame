using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;

namespace FpsGame.Ui
{
    public abstract class Screen : IDisposable
    {
        private bool disposedValue;

        public abstract void SetActive(Desktop desktop);

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
