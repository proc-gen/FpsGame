using FpsGame.Common.Constants;
using FpsGame.Common.Utils;
using FpsGame.Containers;
using FpsGame.Screens;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace FpsGame
{
    public class Game1 : Game
    {
        public GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private ScreenManager screenManager;
        private DepthStencilState depthStencil;

        public SettingsContainer settings;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            LoadUserDefinedSettings();
            InitializeScreenManager();            
            base.Initialize();
        }

        private void LoadUserDefinedSettings()
        {
            Window.AllowUserResizing = false;
            settings = JsonFileManager.LoadFile<SettingsContainer>(GameFiles.Settings, true);

            if (string.IsNullOrEmpty(settings.Resolution))
            {
                settings.Resolution = SettingsScreen.FormatResolution(GraphicsDevice.Adapter.CurrentDisplayMode);
                settings.WindowMode = WindowMode.Fullscreen;
                JsonFileManager.SaveFile(settings, GameFiles.Settings);

                graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                graphics.ToggleFullScreen();
            }
            else
            {
                var displayMode = GraphicsDevice.Adapter.SupportedDisplayModes.Where(a => SettingsScreen.FormatResolution(a) == settings.Resolution).First();
                graphics.PreferredBackBufferWidth = displayMode.Width;
                graphics.PreferredBackBufferHeight = displayMode.Height;

                if (settings.WindowMode == WindowMode.Fullscreen)
                {
                    graphics.ToggleFullScreen();
                }
                else if (settings.WindowMode == WindowMode.BorderlessWindow)
                {
                    Window.IsBorderless = true;
                }
                graphics.ApplyChanges();
            }
        }

        private void InitializeScreenManager()
        {
            screenManager = new ScreenManager(this);
            screenManager.AddScreen(ScreenNames.MainMenu, new MainMenuScreen(this, screenManager));
            screenManager.SetActiveScreen(ScreenNames.MainMenu);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            depthStencil = new DepthStencilState() { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.Less };
        }

        protected override void Update(GameTime gameTime)
        {
            screenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = depthStencil;

            screenManager.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}