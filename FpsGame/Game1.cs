using FpsGame.Common.Constants;
using FpsGame.Screens;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FpsGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private ScreenManager screenManager;
        private DepthStencilState depthStencil;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            graphics.ToggleFullScreen();
            screenManager = new ScreenManager(this);
            screenManager.AddScreen(ScreenNames.MainMenu, new MainMenuScreen(this, screenManager));
            screenManager.SetActiveScreen(ScreenNames.MainMenu);
            base.Initialize();
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