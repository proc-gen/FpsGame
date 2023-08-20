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
        Label playerNameLabel;
        TextBox playerNameTextBox;
        Button continueButton;
        Button backButton;

        public GameSetupScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Game Setup");
            
            continueButton = new Button("continue", "Start Game", ContinueButtonClick);
            backButton = new Button("back", "Back to Main Menu", BackButtonClick);
            
            panel.AddWidget(titleLabel);
            
            if(gameSettings.GameMode != GameMode.MultiplayerJoin)
            {
                gameNameLabel = new Label("game-name-label", "Game Name: ");
                gameNameTextBox = new TextBox("game-name");
                panel.AddWidget(gameNameLabel);
                panel.AddWidget(gameNameTextBox);
            }

            if(gameSettings.GameMode != GameMode.StandaloneServer)
            {
                playerNameLabel = new Label("player-name-label", "Player Name: ");
                playerNameTextBox = new TextBox("player-name");
                panel.AddWidget(playerNameLabel);
                panel.AddWidget(playerNameTextBox);
            }
            
            panel.AddWidget(continueButton);
            panel.AddWidget(backButton);

            RootWidget = panel.UiWidget;
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            continueButton.UiWidget.Enabled = 
                (gameSettings.GameMode == GameMode.MultiplayerJoin 
                || !string.IsNullOrWhiteSpace(gameNameTextBox.Text))
                && (gameSettings.GameMode == GameMode.StandaloneServer
                || !string.IsNullOrWhiteSpace(playerNameTextBox.Text));
        }

        protected void ContinueButtonClick(object e, EventArgs eventArgs)
        {
            if (continueButton.UiWidget.Enabled)
            {
                if (gameSettings.GameMode != GameMode.MultiplayerJoin)
                {
                    gameSettings.GameName = gameNameTextBox.Text;
                }

                PlayerSettings? playerSettings = null;

                if(gameSettings.GameMode != GameMode.StandaloneServer)
                {
                    Random random = new Random();
                    playerSettings = new PlayerSettings()
                    {
                        Name = playerNameTextBox.Text,
                        Color = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()),
                    };
                }

                ScreenManager.AddScreen(ScreenNames.Game, new GameScreen(Game, ScreenManager, gameSettings, playerSettings));
                ScreenManager.SetActiveScreen(ScreenNames.Game);
            }
        }

        protected void BackButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
        }
    }
}
