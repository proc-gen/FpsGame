using FpsGame.Common.Constants;
using FpsGame.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Screens
{
    public class GameScreen : Screen
    {
        public GameScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            // Nothing special
        }
    }
}
