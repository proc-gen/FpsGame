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
using FpsGame.Ui.Styles;
using FontStashSharp;
using System.IO;
using FpsGame.Common.Level;

namespace FpsGame.Screens
{
    public class GameSetupScreen : Screen
    {
        GameSettings gameSettings;

        VerticalPanel panel; 
        Label titleLabel;

        InputWrapper<TextBox, MyraTextBox> gameNameWrapper;
        InputWrapper<TextBox, MyraTextBox> playerNameWrapper;
        InputWrapper<TextBox, MyraTextBox> ipAddressWrapper;
        InputWrapper<ComboBox, MyraCombo> ipAddressSelectionWrapper;
        InputWrapper<TextBox, MyraTextBox> ipAddressPortSelectionWrapper;
        InputWrapper<ComboBox, MyraCombo> levelSelectionWrapper;

        List<ListItem> levels;

        Grid buttonGrid;
        Button continueButton;
        Button backButton;

        public GameSetupScreen(Game game, ScreenManager screenManager, GameSettings gameSettings) 
            : base(game, screenManager)
        {
            this.gameSettings = gameSettings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Game Setup", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(16),
                FontScale = new Vector2(1.5f, 1.5f)
            });
            panel.AddWidget(titleLabel);
            
            createGameNameInput();
            createIpAddressSelection();
            createIpAddressEntry();
            createPlayerNameEntry();
            createLevelSelection();
            createButtons();

            RootWidget = panel.UiWidget;
        }

        private void createGameNameInput()
        {
            if (gameSettings.GameMode == GameMode.MultiplayerHost
                || gameSettings.GameMode == GameMode.StandaloneServer)
            {
                var gameNameLabel = new Label("game-name-label", "Game Name: ");
                var gameNameTextBox = new TextBox("game-name");
                gameNameWrapper = new InputWrapper<TextBox, MyraTextBox>("game-name", gameNameLabel, gameNameTextBox);

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

                var ipAddressSelectionLabel = new Label("ip-address-selection-label", "Hosting Address:");
                var ipAddressSelection = new ComboBox("ip-address-selection", availableIpListItems);
                ipAddressSelection.UiWidget.SelectedIndex = 0;
                ipAddressSelectionWrapper = new InputWrapper<ComboBox, MyraCombo>("ip-address-selection", ipAddressSelectionLabel, ipAddressSelection);
                panel.AddWidget(ipAddressSelectionWrapper.Grid);

                var ipAddressPortSelectionLabel = new Label("ip-address-port-selection-label", "Hosting Port:");
                var ipAddressPort = new TextBox("ip-address-port", "12345");
                ipAddressPortSelectionWrapper = new InputWrapper<TextBox, MyraTextBox>("ip-address-port", ipAddressPortSelectionLabel, ipAddressPort);
                panel.AddWidget(ipAddressPortSelectionWrapper.Grid);
            }
        }

        private void createIpAddressEntry()
        {
            if(gameSettings.GameMode == GameMode.MultiplayerJoin)
            {
                var ipAddressLabel = new Label("ip-address-label", "Host Address: ");
                var ipAddressTextBox = new TextBox("ip-address");
                ipAddressWrapper = new InputWrapper<TextBox, MyraTextBox>("ip-address", ipAddressLabel, ipAddressTextBox);
                panel.AddWidget(ipAddressWrapper.Grid);
            }
        }

        private void createPlayerNameEntry()
        {
            if(gameSettings.GameMode != GameMode.StandaloneServer) 
            {
                var playerNameLabel = new Label("player-name-label", "Player Name: ");
                var playerNameTextBox = new TextBox("player-name");
                playerNameWrapper = new InputWrapper<TextBox, MyraTextBox>("player-name", playerNameLabel, playerNameTextBox);

                panel.AddWidget(playerNameWrapper.Grid);
            }
        }

        private void createLevelSelection()
        {
            if(gameSettings.GameMode != GameMode.MultiplayerJoin)
            {
                var levelSelectionLabel = new Label("level-selection-label", "Level:");
                levels = new List<ListItem>();

                var levelDirectory = new DirectoryInfo("Levels");
                foreach(var file in levelDirectory.GetFiles())
                {
                    var level = new LevelFromFile(null, null, Path.Combine("Levels", file.Name));
                    level.GetLevelInfo();
                    levels.Add(new ListItem(level.LevelData.LevelName, file.Name));
                }

                var levelSelection = new ComboBox("level-selection", levels);
                levelSelection.UiWidget.SelectedIndex = 0;
                levelSelectionWrapper = new InputWrapper<ComboBox, MyraCombo>("level-selection", levelSelectionLabel, levelSelection);
                panel.AddWidget(levelSelectionWrapper.Grid);
            }
        }

        private void createButtons()
        {
            continueButton = new Button("continue", gameSettings.GameMode == GameMode.MultiplayerJoin ? "Join Game" : "Start Game", ContinueButtonClick);
            continueButton.UiWidget.GridColumn = 0;
            backButton = new Button("back", "Back to Main Menu", BackButtonClick);
            backButton.UiWidget.GridColumn = 1;

            buttonGrid = new Grid("button-grid");
            buttonGrid.AddWidget(continueButton);
            buttonGrid.AddWidget(backButton);

            panel.AddWidget(buttonGrid);
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            continueButton.UiWidget.Enabled = 
                (gameSettings.GameMode == GameMode.SinglePlayer || gameSettings.GameMode == GameMode.MultiplayerJoin 
                    || !string.IsNullOrWhiteSpace(gameNameWrapper.InputComponent.Text))
                && (gameSettings.GameMode == GameMode.SinglePlayer || gameSettings.GameMode == GameMode.MultiplayerJoin
                    || (IPAddress.TryParse(ipAddressSelectionWrapper.InputComponent.UiWidget.SelectedItem.Text, out IPAddress _dummy) 
                        && int.TryParse(ipAddressPortSelectionWrapper.InputComponent.Text, out int _dummyPort)))
                && (gameSettings.GameMode != GameMode.MultiplayerJoin
                    || (ipAddressWrapper.InputComponent.Text.Contains(':')
                        && IPAddress.TryParse(ipAddressWrapper.InputComponent.Text.Split(':')[0], out IPAddress _dummy2)
                        && int.TryParse(ipAddressWrapper.InputComponent.Text.Split(':')[1], out int _dummyPort2)))
                && (gameSettings.GameMode == GameMode.StandaloneServer
                    || !string.IsNullOrWhiteSpace(playerNameWrapper.InputComponent.Text));
        }

        protected void ContinueButtonClick(object e, EventArgs eventArgs)
        {
            if (continueButton.UiWidget.Enabled)
            {
                if(gameSettings.GameMode == GameMode.SinglePlayer)
                {
                    gameSettings.GameName = string.Empty;
                    gameSettings.GameIPAddress = IPAddress.Loopback;
                    gameSettings.GamePort = 12345;
                    gameSettings.LevelFile = levels[levelSelectionWrapper.InputComponent.UiWidget.SelectedIndex.GetValueOrDefault(0)].Value;
                }
                else if (gameSettings.GameMode != GameMode.MultiplayerJoin)
                {
                    gameSettings.GameName = gameNameWrapper.InputComponent.UiWidget.Text;
                    gameSettings.GameIPAddress = IPAddress.Parse(ipAddressSelectionWrapper.InputComponent.UiWidget.SelectedItem.Text);
                    gameSettings.GamePort = int.Parse(ipAddressPortSelectionWrapper.InputComponent.Text);
                    gameSettings.LevelFile = levels[levelSelectionWrapper.InputComponent.UiWidget.SelectedIndex.GetValueOrDefault(0)].Value;
                }
                else
                {
                    string[] ipAddress = ipAddressWrapper.InputComponent.Text.Split(':');
                    gameSettings.GameIPAddress = IPAddress.Parse(ipAddress[0]);
                    gameSettings.GamePort = int.Parse(ipAddress[1]);
                }

                if(gameSettings.GameMode != GameMode.StandaloneServer)
                {
                    Random random = new Random();
                    PlayerSettings playerSettings = new PlayerSettings()
                    {
                        Name = playerNameWrapper.InputComponent.Text,
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
