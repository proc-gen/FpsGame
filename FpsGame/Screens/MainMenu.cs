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
        VerticalPanel panel;
        Label label;
        Button newGameButton;
        Button exitButton;

        public MainMenu(Game game)
            : base(game)
        {
            panel = new VerticalPanel("panel");
            label = new Label("title", "Main Menu");
            newGameButton = new Button("new-game", "New Game", NewGameButtonClick);
            exitButton = new Button("exit", "Exit", ExitButtonClick);
            
            panel.AddWidget(label);
            panel.AddWidget(newGameButton);
            panel.AddWidget(exitButton);

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

        protected void NewGameButtonClick(object e, EventArgs eventArgs)
        {
            // Nothing for now
        }

        protected void ExitButtonClick(object e, EventArgs eventArgs)
        {
            Game.Exit();
        }
    }
}
