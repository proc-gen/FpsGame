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
        Panel panel;
        Label label;
        Button button;

        public MainMenu(Game game)
            : base(game)
        {
            panel = new Panel("panel");
            label = new Label("title", "Main Menu");
            button = new Button("exit", "Exit", ButtonClick);
            panel.AddWidget(label);
            panel.AddWidget(button);
            RootWidget = panel.UiWidget;
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
