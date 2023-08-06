using FpsGame.Ui;
using FpsGame.Ui.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Screens
{
    public class MainMenu : Screen
    {
        Grid grid;
        Label label;
        Button button;

        public MainMenu(Game game)
            : base(game)
        {
            grid = new Grid();
            label = new Label("title", "Main Menu");
            button = new Button("exit", "Exit", ButtonClick);
            grid.AddWidget(label);
            grid.AddWidget(button);
            RootWidget = grid.UiWidget;
        }

        public override void Update(GameTime gameTime)
        {
            // Nothing special
        }

        public override void Draw(GameTime gameTime)
        {
            // Nothing special
        }

        protected void ButtonClick(object e, EventArgs eventArgs)
        {
            Game.Exit();
        }
    }
}
