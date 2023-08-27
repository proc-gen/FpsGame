using FpsGame.Common.Constants;
using FpsGame.Common.ClientData;
using FpsGame.Server.Utils;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

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
        Label ipAddressLabel;
        TextBox ipAddressTextBox;
        Label ipAddressSelectionLabel;
        ComboBox ipAddressSelection;
        Button continueButton;
        Button backButton;

        public GameSetupScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Game Setup");
            
            continueButton = new Button("continue", gameSettings.GameMode == GameMode.MultiplayerJoin ? "Join Game" : "Start Game", ContinueButtonClick);
            backButton = new Button("back", "Back to Main Menu", BackButtonClick);
            
            panel.AddWidget(titleLabel);
            
            if(gameSettings.GameMode != GameMode.MultiplayerJoin)
            {
                gameNameLabel = new Label("game-name-label", "Game Name: ");
                gameNameTextBox = new TextBox("game-name");
                panel.AddWidget(gameNameLabel);
                panel.AddWidget(gameNameTextBox);

                if(gameSettings.GameMode != GameMode.SinglePlayer)
                {
                    var availableIps = IPAddressUtils.GetAllLocalIPv4();
                    List<ListItem> availableIpListItems = new List<ListItem>();

                    foreach (var ip in availableIps)
                    {
                        availableIpListItems.Add(new ListItem(ip.ToString()));
                    }

                    ipAddressSelectionLabel = new Label("ip-address-selection-label", "Hosting address:");
                    ipAddressSelection = new ComboBox("ip-address-selection", availableIpListItems);
                    ipAddressSelection.UiWidget.SelectedIndex = 0;

                    panel.AddWidget(ipAddressSelectionLabel);
                    panel.AddWidget(ipAddressSelection);
                }
            }
            else
            {
                ipAddressLabel = new Label("ip-address-label", "Host Address: ");
                ipAddressTextBox = new TextBox("ip-address");
                panel.AddWidget(ipAddressLabel);
                panel.AddWidget(ipAddressTextBox);
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
            IPAddress dummy = null;
            int dummyPort;
            continueButton.UiWidget.Enabled = 
                (gameSettings.GameMode == GameMode.MultiplayerJoin 
                    || !string.IsNullOrWhiteSpace(gameNameTextBox.Text))
                && (gameSettings.GameMode != GameMode.MultiplayerJoin
                    || (ipAddressTextBox.Text.Contains(':')
                        && IPAddress.TryParse(ipAddressTextBox.Text.Split(':')[0], out dummy)
                        && int.TryParse(ipAddressTextBox.Text.Split(':')[1], out dummyPort)))
                && (gameSettings.GameMode == GameMode.StandaloneServer
                    || !string.IsNullOrWhiteSpace(playerNameTextBox.Text));
        }

        protected void ContinueButtonClick(object e, EventArgs eventArgs)
        {
            if (continueButton.UiWidget.Enabled)
            {
                if(gameSettings.GameMode == GameMode.SinglePlayer)
                {
                    gameSettings.GameName = gameNameTextBox.Text;
                    gameSettings.GameIPAddress = IPAddress.Loopback;
                    gameSettings.GamePort = 12345;
                }
                else if (gameSettings.GameMode != GameMode.MultiplayerJoin)
                {
                    gameSettings.GameName = gameNameTextBox.Text;
                    gameSettings.GameIPAddress = IPAddress.Parse(ipAddressSelection.UiWidget.SelectedItem.Text);
                    gameSettings.GamePort = 12345;
                }
                else
                {
                    string[] ipAddress = ipAddressTextBox.Text.Split(':');
                    gameSettings.GameIPAddress = IPAddress.Parse(ipAddress[0]);
                    gameSettings.GamePort = int.Parse(ipAddress[1]);
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
