using FpsGame.Common.Constants;
using FpsGame.Common.Containers;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using Microsoft.Xna.Framework;
using System;

namespace FpsGame.Screens
{
    public class GameSetupScreen : Screen
    {
        GameSettings gameSettings;

        VerticalPanel panel; 
        Label titleLabel;
        Label gameNameLabel;
        TextBox gameNameTextBox;
        Button continueButton;
        Button backButton;

        public GameSetupScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Game Setup");
            gameNameLabel = new Label("game-name-label", "Game Name: ");
            gameNameTextBox = new TextBox("game-name");
            continueButton = new Button("continue", "Start Game", ContinueButtonClick);
            backButton = new Button("back", "Back to Main Menu", BackButtonClick);
            
            panel.AddWidget(titleLabel);
            panel.AddWidget(gameNameLabel);
            panel.AddWidget(gameNameTextBox);
            panel.AddWidget(continueButton);
            panel.AddWidget(backButton);

            RootWidget = panel.UiWidget;
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            continueButton.UiWidget.Enabled = !string.IsNullOrWhiteSpace(gameNameTextBox.Text);
        }

        protected void ContinueButtonClick(object e, EventArgs eventArgs)
        {
            gameSettings.GameName = gameNameTextBox.Text;
            ScreenManager.AddScreen(ScreenNames.Game, new GameScreen(Game, ScreenManager, gameSettings));
            ScreenManager.SetActiveScreen(ScreenNames.Game);
        }

        protected void BackButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
        }
    }
}
