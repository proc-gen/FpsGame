using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace FpsGame.Ui
{
    public class ScreenManager : IDisposable
    {
        private bool disposedValue;
        public Desktop Desktop;

        private Dictionary<string, Screen> screens;

        public string ActiveScreen { get; private set; }

        public ScreenManager(Game game)
        {
            MyraEnvironment.Game = game;
            screens = new Dictionary<string, Screen>();
            Desktop = new Desktop();
        }

        public void Update(GameTime gameTime)
        {
            if(!string.IsNullOrEmpty(ActiveScreen))
            {
                screens[ActiveScreen].Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (!string.IsNullOrEmpty(ActiveScreen))
            {
                screens[ActiveScreen].Render(gameTime);
            }

            Desktop.Render();
        }

        public void AddScreen(string screenName, Screen screen)
        {
            if (!screens.ContainsKey(screenName))
            {
                screens.Add(screenName, screen);
            }
        }

        public void RemoveScreen(string screenName)
        {
            if(HasScreen(screenName))
            {
                screens[screenName].Dispose();
                screens.Remove(screenName);
            }
        }

        public bool HasScreen(string screenName)
        {
            return screens.ContainsKey(screenName);
        }

        public void SetActiveScreen(string screenName)
        {
            ActiveScreen = screenName;
            screens[screenName].SetActive();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(screens.Any())
                    {
                        foreach(var screen in screens)
                        {
                            screen.Value.Dispose();
                        }
                    }
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
