using FpsGame.Common.ClientData;
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
        Button multiPlayerHostButton;
        Button multiplayerJoinButton;
        Button standaloneServerButton;
        Button exitButton;

        public MainMenuScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {
            panel = new VerticalPanel("panel");
            label = new Label("title", "Main Menu");
            newGameButton = new Button("new-game", "New Singleplayer Game", NewGameButtonClick);
            multiPlayerHostButton = new Button("multiplayer-host", "Host Multiplayer Game", MultiplayerHostButtonClick);
            multiplayerJoinButton = new Button("multiplayer-join", "Join Multiplayer Game", MultiplayerJoinButtonClick);
            standaloneServerButton = new Button("standalone-server", "Standalone Server", StandaloneServerButtonClick);
            exitButton = new Button("exit", "Exit", ExitButtonClick);
            
            panel.AddWidget(label);
            panel.AddWidget(newGameButton);
            panel.AddWidget(multiPlayerHostButton);
            panel.AddWidget(multiplayerJoinButton);
            panel.AddWidget(standaloneServerButton);
            panel.AddWidget(exitButton);

            RootWidget = panel.UiWidget;
        }

        public override void SetActive()
        {
            ScreenManager.RemoveAllExcept(ScreenNames.MainMenu);
            base.SetActive();
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        protected void NewGameButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.AddScreen(ScreenNames.GameSetup, new GameSetupScreen(Game, ScreenManager, new GameSettings() { GameMode = GameMode.SinglePlayer }));
            ScreenManager.SetActiveScreen(ScreenNames.GameSetup);
        }

        protected void MultiplayerHostButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.AddScreen(ScreenNames.GameSetup, new GameSetupScreen(Game, ScreenManager, new GameSettings() { GameMode = GameMode.MultiplayerHost }));
            ScreenManager.SetActiveScreen(ScreenNames.GameSetup);
        }

        protected void MultiplayerJoinButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.AddScreen(ScreenNames.GameSetup, new GameSetupScreen(Game, ScreenManager, new GameSettings() { GameMode = GameMode.MultiplayerJoin }));
            ScreenManager.SetActiveScreen(ScreenNames.GameSetup);
        }

        protected void StandaloneServerButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.AddScreen(ScreenNames.GameSetup, new GameSetupScreen(Game, ScreenManager, new GameSettings() { GameMode = GameMode.StandaloneServer }));
            ScreenManager.SetActiveScreen(ScreenNames.GameSetup);
        }

        protected void ExitButtonClick(object e, EventArgs eventArgs)
        {
            Game.Exit();
        }
    }
}
