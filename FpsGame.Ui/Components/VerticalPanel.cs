using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Graphics2D.UI;
using MyraPanel = Myra.Graphics2D.UI.VerticalStackPanel;

namespace FpsGame.Ui.Components
{
    public class VerticalPanel : Component<MyraPanel>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Thickness(4),
        };

        public VerticalPanel(string id)
            : base(id)
        { 
            UiWidget = new MyraPanel()
            {
                Id = id,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            UpdateStyle(baseStyle);
        }

        public void AddWidget<T>(Component<T> component)
            where T : Widget
        {
            UiWidget.Widgets.Add(component.UiWidget);
        }
    }
}
