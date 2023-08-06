﻿using System;
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
        private Desktop desktop;

        private Dictionary<string, Screen> screens;

        public string ActiveScreen { get; set; }

        public ScreenManager(Game game)
        {
            MyraEnvironment.Game = game;
            screens = new Dictionary<string, Screen>();
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
                screens[ActiveScreen].Draw(gameTime);
            }
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
            if(screens.ContainsKey(screenName))
            {
                screens[screenName].Dispose();
                screens.Remove(screenName);
            }
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