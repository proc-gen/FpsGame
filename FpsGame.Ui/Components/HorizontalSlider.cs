using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Utility;
using MyraHorizontalSlider = Myra.Graphics2D.UI.HorizontalSlider;
using MyraGrid = Myra.Graphics2D.UI.Grid;

namespace FpsGame.Ui.Components
{
    public class HorizontalSlider : Component<MyraGrid>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Styles.Thickness(4),
        };

        private float ConversionFactor;
        private string formatting;

        private Grid Grid;
        private Label ValueLabel;
        private MyraHorizontalSlider Slider;

        public float Value 
        { 
            get
            {
                return Slider.Value * ConversionFactor;
            } 
        }

        public HorizontalSlider(string id) 
            : base(id)
        {
            Init(0, 10, 1, 1, string.Empty);
        }

        public HorizontalSlider(string id, int min, int max, float conversionFactor, float initialValue, string formatting = "")
            : base(id)
        {
            Init(min, max, conversionFactor, initialValue, formatting);
        }

        private void Init(int min, int max, float conversionFactor, float initialValue, string formatting)
        {
            this.formatting = formatting;
            Grid = new Grid(Id + "-grid");
            Grid.UiWidget.MinWidth = 200;
            UiWidget = Grid.UiWidget;

            ValueLabel = new Label(Id + "-value-label", initialValue.ToString(formatting));
            ValueLabel.UiWidget.GridRow = 0;
            ValueLabel.UiWidget.GridColumn = 1;
            ValueLabel.UiWidget.Width = 100;

            Slider = new MyraHorizontalSlider()
            {
                Id = Id,
                Minimum = min,
                Maximum = max,
            };
            Slider.GridRow = 0;
            Slider.GridColumn = 0;
            Slider.Width = 100;

            ConversionFactor = conversionFactor;
            Slider.Value = initialValue / conversionFactor;
            Slider.ValueChanged += OnValueChanged;
            Grid.AddWidget(Slider);
            Grid.AddWidget(ValueLabel);
        }

        public void AddOnValueChanged(EventHandler<ValueChangedEventArgs<float>> eventHandler)
        {
            Slider.ValueChanged += eventHandler;
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs<float> e)
        {
            if (sender != UiWidget)
            {
                Slider.Value = MathF.Round(e.NewValue);
                ValueLabel.UpdateText(Value.ToString(formatting));
            }
        }
    }
}
