using FpsGame.Common.Constants;
using FpsGame.Ui;
using FpsGame.Ui.Components;
using FpsGame.Ui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyraComboBox = Myra.Graphics2D.UI.ComboBox;

namespace FpsGame.Screens
{
    public class SettingsScreen : Screen
    {
        VerticalPanel panel;
        Label titleLabel;
        InputWrapper<ComboBox, MyraComboBox> screenResolutions;
        Button backButton;

        static Style ButtonStyle = new Style()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(4),
            Padding = new Thickness(4),
        };

        public SettingsScreen(Game game, ScreenManager screenManager) 
            : base(game, screenManager)
        {
            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Settings", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(16),
                FontScale = new Vector2(1.5f, 1.5f)
            });
            panel.AddWidget(titleLabel);

            var resolutions = game.GraphicsDevice.Adapter.SupportedDisplayModes;
            var currentResolution = game.GraphicsDevice.Adapter.CurrentDisplayMode;

            List<ListItem> availableResolutions = new List<ListItem>();
            foreach (var resolution in resolutions.OrderByDescending(a => a.Width).ThenByDescending(a => a.Height))
            {
                if (resolution.Width > 1000)
                {
                    availableResolutions.Add(new ListItem(formatResolution(resolution)));
                }
            }

            var resolutionLabel = new Label("resolution-label", "Resolution");
            var resolutionSelection = new ComboBox("resolution-selection", availableResolutions);
            resolutionSelection.UiWidget.SelectedIndex = availableResolutions.IndexOf(availableResolutions.Where(a => a.MyraListItem.Text == formatResolution(currentResolution)).First());
            screenResolutions = new InputWrapper<ComboBox, MyraComboBox>("resolution", resolutionLabel, resolutionSelection);
            panel.AddWidget(screenResolutions.Grid);

            backButton = new Button("back", "Back to Main Menu", BackButtonClick, ButtonStyle);
            panel.AddWidget(backButton);

            RootWidget = panel.UiWidget;
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        protected void BackButtonClick(object e, EventArgs eventArgs)
        {
            ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
        }

        private string formatResolution(DisplayMode resolution)
        {
            return $"{resolution.Width} x {resolution.Height}";
        }
    }
}
