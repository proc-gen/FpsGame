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
            if(!string.IsNullOrEmpty(ActiveScreen) && screens.ContainsKey(ActiveScreen))
            {
                screens[ActiveScreen].Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (!string.IsNullOrEmpty(ActiveScreen) && screens.ContainsKey(ActiveScreen))
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
                var disposeMethod = screens[screenName].GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(screens[screenName], null);
                screens.Remove(screenName);
            }
        }

        public void RemoveAllExcept(string screenName)
        {
            foreach(var screen in screens)
            {
                if(screen.Key != screenName)
                {
                    var disposeMethod = screen.Value.GetType().GetMethod("Dispose");
                    disposeMethod?.Invoke(screen.Value, null);
                    screens.Remove(screen.Key);
                }
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
                            var disposeMethod = screen.GetType().GetMethod("Dispose");
                            disposeMethod?.Invoke(screen, null);
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
