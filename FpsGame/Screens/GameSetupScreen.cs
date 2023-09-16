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
using MyraTextBox = Myra.Graphics2D.UI.TextBox;
using MyraCombo = Myra.Graphics2D.UI.ComboBox;

namespace FpsGame.Screens
{
    public class GameSetupScreen : Screen
    {
        GameSettings gameSettings;

        VerticalPanel panel; 
        Label titleLabel;
        InputWrapper<Component<MyraTextBox>, MyraTextBox> gameNameWrapper;
        Label gameNameLabel;
        TextBox gameNameTextBox;

        InputWrapper<Component<MyraTextBox>, MyraTextBox> playerNameWrapper;
        Label playerNameLabel;
        TextBox playerNameTextBox;

        InputWrapper<Component<MyraTextBox>, MyraTextBox> ipAddressWrapper;
        Label ipAddressLabel;
        TextBox ipAddressTextBox;

        InputWrapper<Component<MyraCombo>, MyraCombo> ipAddressSelectionWrapper;
        Label ipAddressSelectionLabel;
        ComboBox ipAddressSelection;

        InputWrapper<Component<MyraTextBox>, MyraTextBox> ipAddressPortSelectionWrapper;
        Label ipAddressPortSelectionLabel;
        TextBox ipAddressPort;
        
        Button continueButton;
        Button backButton;

        public GameSetupScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Game Setup");
                        
            panel.AddWidget(titleLabel);
            createGameNameInput();
            createIpAddressSelection();
            createIpAddressEntry();
            createPlayerNameEntry();
            createButtons();

            RootWidget = panel.UiWidget;
        }

        private void createGameNameInput()
        {
            if (gameSettings.GameMode == GameMode.MultiplayerHost
                || gameSettings.GameMode == GameMode.StandaloneServer)
            {
                gameNameLabel = new Label("game-name-label", "Game Name: ");
                gameNameTextBox = new TextBox("game-name");
                gameNameWrapper = new InputWrapper<Component<MyraTextBox>, MyraTextBox>("game-name", gameNameLabel, gameNameTextBox);

                panel.AddWidget(gameNameWrapper.Grid);
            }
        }

        private void createIpAddressSelection()
        {
            if (gameSettings.GameMode == GameMode.MultiplayerHost
                || gameSettings.GameMode == GameMode.StandaloneServer)
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
                ipAddressSelectionWrapper = new InputWrapper<Component<MyraCombo>, MyraCombo>("ip-address-selection", ipAddressSelectionLabel, ipAddressSelection);
                panel.AddWidget(ipAddressSelectionWrapper.Grid);

                ipAddressPortSelectionLabel = new Label("ip-address-port-selection-label", "Hosting port:");
                ipAddressPort = new TextBox("ip-address-port", "12345");
                ipAddressPortSelectionWrapper = new InputWrapper<Component<MyraTextBox>, MyraTextBox>("ip-address-port", ipAddressPortSelectionLabel, ipAddressPort);
                panel.AddWidget(ipAddressPortSelectionWrapper.Grid);
            }
        }

        private void createIpAddressEntry()
        {
            if(gameSettings.GameMode == GameMode.MultiplayerJoin)
            {
                ipAddressLabel = new Label("ip-address-label", "Host Address: ");
                ipAddressTextBox = new TextBox("ip-address");
                panel.AddWidget(ipAddressLabel);
                panel.AddWidget(ipAddressTextBox);
            }
        }

        private void createPlayerNameEntry()
        {
            if(gameSettings.GameMode != GameMode.StandaloneServer) 
            {
                playerNameLabel = new Label("player-name-label", "Player Name: ");
                playerNameTextBox = new TextBox("player-name");
                panel.AddWidget(playerNameLabel);
                panel.AddWidget(playerNameTextBox);
            }
        }

        private void createButtons()
        {
            continueButton = new Button("continue", gameSettings.GameMode == GameMode.MultiplayerJoin ? "Join Game" : "Start Game", ContinueButtonClick);
            backButton = new Button("back", "Back to Main Menu", BackButtonClick);

            panel.AddWidget(continueButton);
            panel.AddWidget(backButton);
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
                && (gameSettings.GameMode == GameMode.SinglePlayer || gameSettings.GameMode == GameMode.MultiplayerJoin
                    || (IPAddress.TryParse(ipAddressSelection.UiWidget.SelectedItem.Text, out dummy) 
                        && int.TryParse(ipAddressPort.Text, out dummyPort)))
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
                    gameSettings.GamePort = int.Parse(ipAddressPort.Text);
                }
                else
                {
                    string[] ipAddress = ipAddressTextBox.Text.Split(':');
                    gameSettings.GameIPAddress = IPAddress.Parse(ipAddress[0]);
                    gameSettings.GamePort = int.Parse(ipAddress[1]);
                }

                if(gameSettings.GameMode != GameMode.StandaloneServer)
                {
                    Random random = new Random();
                    PlayerSettings playerSettings = new PlayerSettings()
                    {
                        Name = playerNameTextBox.Text,
                        Color = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()),
                    };

                    ScreenManager.AddScreen(ScreenNames.Game, new GameScreen(Game, ScreenManager, gameSettings, playerSettings));
                    ScreenManager.SetActiveScreen(ScreenNames.Game);
                }
                else
                {
                    ScreenManager.AddScreen(ScreenNames.Server, new ServerScreen(Game, ScreenManager, gameSettings));
                    ScreenManager.SetActiveScreen(ScreenNames.Server);
                }
                
                
            }
        }

        protected void BackButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
        }
    }
}
