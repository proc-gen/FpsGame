using FpsGame.Common.Constants;
using FpsGame.Common.Utils;
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
using MyraHorizontalSlider = Myra.Graphics2D.UI.HorizontalSlider;
using MyraGrid = Myra.Graphics2D.UI.Grid;
using FpsGame.Containers;
using Myra.Utility;

namespace FpsGame.Screens
{
    public class SettingsScreen : Screen
    {
        VerticalPanel panel;
        Label titleLabel;

        InputWrapper<ComboBox, MyraComboBox> screenResolutionsWrapper;
        DisplayModeCollection resolutions;
        DisplayMode currentResolution;

        InputWrapper<ComboBox, MyraComboBox> windowModeWrapper;
        string currentWindowMode;

        InputWrapper<HorizontalSlider, MyraGrid> mouseSensitivityWrapper;
        float mouseSensitivity;

        InputWrapper<HorizontalSlider, MyraGrid> controllerSensitivityWrapper;
        float controllerSensitivity;

        Grid buttonGrid;
        Button applyButton;
        Button backButton;

        SettingsContainer settings;

        static Style ButtonStyle = new Style()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(4),
            Padding = new Thickness(4),
        };

        public SettingsScreen(Game game, ScreenManager screenManager) 
            : base(game, screenManager)
        {
            settings = ((Game1)Game).settings;

            panel = new VerticalPanel("panel");
            titleLabel = new Label("title", "Settings", new Style()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(16),
                FontScale = new Vector2(1.5f, 1.5f)
            });
            panel.AddWidget(titleLabel);

            createResolutionSelection();
            createWindowModeSelection();
            createMouseSensitivitySlider();
            createControllerSensitivitySlider();
            createButtons();            

            RootWidget = panel.UiWidget;
        }

        private void createResolutionSelection()
        {
            resolutions = Game.GraphicsDevice.Adapter.SupportedDisplayModes;

            List<ListItem> availableResolutions = new List<ListItem>();
            foreach (var resolution in resolutions.OrderByDescending(a => a.Width).ThenByDescending(a => a.Height))
            {
                if (resolution.Width > 1000)
                {
                    availableResolutions.Add(new ListItem(FormatResolution(resolution)));
                }
            }

            var currentWidth = ((Game1)Game).graphics.PreferredBackBufferWidth;
            var currentHeight = ((Game1)Game).graphics.PreferredBackBufferHeight;

            currentResolution = resolutions.Where(a => FormatResolution(a) == FormatResolution(currentWidth, currentHeight)).First();

            var resolutionLabel = new Label("resolution-label", "Resolution");
            var resolutionSelection = new ComboBox("resolution-selection", availableResolutions);
            resolutionSelection.UiWidget.SelectedIndex = availableResolutions.IndexOf(availableResolutions.Where(a => a.MyraListItem.Text == FormatResolution(currentResolution)).First());
            screenResolutionsWrapper = new InputWrapper<ComboBox, MyraComboBox>("resolution", resolutionLabel, resolutionSelection);
            panel.AddWidget(screenResolutionsWrapper.Grid);
        }

        private void createWindowModeSelection()
        {
            currentWindowMode = settings.WindowMode;

            List<ListItem> windowModes = new List<ListItem>()
            {
                new ListItem(WindowMode.Fullscreen),
                new ListItem(WindowMode.Window),
                new ListItem(WindowMode.BorderlessWindow),
            };

            var windowModeLabel = new Label("window-mode-label", "Window Mode");
            var windowModeSelection = new ComboBox("window-mode-selection", windowModes);
            windowModeSelection.UiWidget.SelectedIndex = windowModes.IndexOf(windowModes.Where(a => a.MyraListItem.Text == currentWindowMode.ToString()).First());
            windowModeWrapper = new InputWrapper<ComboBox, MyraComboBox>("window-mode", windowModeLabel, windowModeSelection);
            panel.AddWidget(windowModeWrapper.Grid);
        }

        private void createMouseSensitivitySlider()
        {
            mouseSensitivity = settings.MouseSensitivity;
            var mouseSensitivityLabel = new Label("mouse-sensitivity-label", "Mouse Sensitivity");
            var mouseSensitivitySlider = new HorizontalSlider("mouse-sensitivity-slider", 1, 100, 0.01f, mouseSensitivity, "P0");
            mouseSensitivitySlider.AddOnValueChanged(OnMouseSensitivityChanged);
            mouseSensitivityWrapper = new InputWrapper<HorizontalSlider, MyraGrid>("mouse-sensitivity", mouseSensitivityLabel, mouseSensitivitySlider);
            
            panel.AddWidget(mouseSensitivityWrapper.Grid);
        }

        private void createControllerSensitivitySlider()
        {
            controllerSensitivity = settings.ControllerSensitivity;
            var controllerSensitivityLabel = new Label("controller-sensitivity-label", "Controller Sensitivity");
            var controllerSensitivitySlider = new HorizontalSlider("controller-sensitivity-slider", 1, 20, 0.5f, controllerSensitivity, "0.#");
            controllerSensitivitySlider.AddOnValueChanged(OnControllerSensitivityChanged);
            controllerSensitivityWrapper = new InputWrapper<HorizontalSlider, MyraGrid>("controller-sensitivity", controllerSensitivityLabel, controllerSensitivitySlider);
            panel.AddWidget(controllerSensitivityWrapper.Grid);
        }

        private void createButtons()
        {
            applyButton = new Button("apply", "Apply", ApplyButtonClick);
            applyButton.UiWidget.GridColumn = 0;
            applyButton.UiWidget.Enabled = false;
            backButton = new Button("back", "Back to Main Menu", BackButtonClick, ButtonStyle);
            backButton.UiWidget.GridColumn = 1;
            buttonGrid = new Grid("button-grid");
            buttonGrid.AddWidget(applyButton);
            buttonGrid.AddWidget(backButton);
            panel.AddWidget(buttonGrid);
        }

        public override void Render(GameTime gameTime)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            var selectedResolution = resolutions.Where(a => FormatResolution(a) == screenResolutionsWrapper.InputComponent.UiWidget.SelectedItem.Text).FirstOrDefault();
            var selectedWindowMode = windowModeWrapper.InputComponent.UiWidget.SelectedItem.Text;
            if (selectedResolution != currentResolution
                || selectedWindowMode != currentWindowMode)
            {
                applyButton.UiWidget.Enabled = true;
            }
        }

        protected void ApplyButtonClick(object e, EventArgs eventArgs)
        {
            if(applyButton.UiWidget.Enabled)
            {
                var selectedResolution = resolutions.Where(a => FormatResolution(a) == screenResolutionsWrapper.InputComponent.UiWidget.SelectedItem.Text).FirstOrDefault();
                var selectedWindowMode = windowModeWrapper.InputComponent.UiWidget.SelectedItem.Text;

                if (selectedResolution != currentResolution)
                {
                    ((Game1)Game).graphics.PreferredBackBufferWidth = selectedResolution.Width;
                    ((Game1)Game).graphics.PreferredBackBufferHeight = selectedResolution.Height;
                    ((Game1)Game).graphics.ApplyChanges();
                    currentResolution = selectedResolution;
                    settings.Resolution = FormatResolution(currentResolution);
                }

                if(selectedWindowMode != currentWindowMode)
                {
                    if(selectedWindowMode == WindowMode.Fullscreen ||
                        currentWindowMode == WindowMode.Fullscreen)
                    {
                        ((Game1)Game).graphics.ToggleFullScreen();
                    }

                    if(selectedWindowMode == WindowMode.BorderlessWindow)
                    {
                        Game.Window.IsBorderless = true;
                    }
                    else if(selectedWindowMode == WindowMode.Window)
                    {
                        Game.Window.IsBorderless = false;
                    }

                    currentWindowMode = selectedWindowMode;
                    settings.WindowMode = currentWindowMode;
                }

                applyButton.UiWidget.Enabled = false;
            }
        }

        protected void BackButtonClick(object e, EventArgs eventArgs)
        {
            JsonFileManager.SaveFile(settings, GameFiles.Settings);
            ScreenManager.SetActiveScreen(ScreenNames.MainMenu);
        }

        protected void OnMouseSensitivityChanged(object sender, ValueChangedEventArgs<float> e)
        {
            settings.MouseSensitivity = mouseSensitivityWrapper.InputComponent.Value;
        }

        protected void OnControllerSensitivityChanged(object sender, ValueChangedEventArgs<float> e)
        {
            settings.ControllerSensitivity = controllerSensitivityWrapper.InputComponent.Value;
        }

        public static string FormatResolution(DisplayMode resolution)
        {
            return FormatResolution(resolution.Width, resolution.Height);
        }

        public static string FormatResolution(int width, int height)
        {
            return $"{width} x {height}";
        }
    }
}
