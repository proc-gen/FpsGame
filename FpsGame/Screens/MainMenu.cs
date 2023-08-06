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

        public MainMenu()
        {
            grid = new Grid();
            label = new Label("title", "Main Menu");
            grid.AddWidget(label);
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
    }
}
