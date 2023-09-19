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
    public class SettingsScreen : Screen
    {
        VerticalPanel panel;
        Label titleLabel;

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
    }
}
