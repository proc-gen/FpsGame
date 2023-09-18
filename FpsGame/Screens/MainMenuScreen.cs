using FpsGame.Common.ClientData;
using FpsGame.Common.Constants;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using FpsGame.Ui.Styles;
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
        Label titleLabel;
        Button newGameButton;
        Button multiPlayerHostButton;
        Button multiplayerJoinButton;
        Button standaloneServerButton;
        Button exitButton;

        static Style ButtonStyle = new Style()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(4),
            Padding = new Thickness(4),
        };

        public MainMenuScreen(Game game, ScreenManager screenManager)
            : base(game, screenManager)
        {
            panel = new VerticalPanel("panel");
            titleLabel = new Label("game-title", "Proc-Gen's Multiplayer FPS Sandbox", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(32),
                FontScale = new Vector2(2.5f, 2.5f)
            });

            newGameButton = new Button("new-game", "New Singleplayer Game", NewGameButtonClick, ButtonStyle);
            multiPlayerHostButton = new Button("multiplayer-host", "Host Multiplayer Game", MultiplayerHostButtonClick, ButtonStyle);
            multiplayerJoinButton = new Button("multiplayer-join", "Join Multiplayer Game", MultiplayerJoinButtonClick, ButtonStyle);
            standaloneServerButton = new Button("standalone-server", "Standalone Server", StandaloneServerButtonClick, ButtonStyle);
            exitButton = new Button("exit", "Exit", ExitButtonClick, ButtonStyle);
            
            panel.AddWidget(titleLabel);
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
