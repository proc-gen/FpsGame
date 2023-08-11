using FpsGame.Common.Constants;
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
    public class MainMenuScreen : Screen
    {
        VerticalPanel panel;
        Label label;
        Button newGameButton;
        Button exitButton;

        public MainMenuScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
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

        public override void SetActive()
        {
            ScreenManager.RemoveScreen(ScreenNames.Game);
            base.SetActive();
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Render(GameTime gameTime)
        {
            // Nothing special
        }

        protected void NewGameButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.AddScreen(ScreenNames.Game, new GameScreen(Game, ScreenManager));
            ScreenManager.SetActiveScreen(ScreenNames.Game);
        }

        protected void ExitButtonClick(object e, EventArgs eventArgs)
        {
            Game.Exit();
        }
    }
}
