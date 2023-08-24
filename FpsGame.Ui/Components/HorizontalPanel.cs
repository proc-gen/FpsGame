using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Graphics2D.UI;
using MyraPanel = Myra.Graphics2D.UI.HorizontalStackPanel;

namespace FpsGame.Ui.Components
{
    public class HorizontalPanel : Component<MyraPanel>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Thickness(4),
        };

        public HorizontalPanel(string id)
            : base(id)
        { 
            UiWidget = new MyraPanel()
            {
                Id = id,
                Spacing = 4,
                HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center,
                VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center,
            };

            UpdateStyle(baseStyle);
        }

        public HorizontalPanel(string id, Style style)
            : base(id)
        {
            UiWidget = new MyraPanel()
            {
                Id = id,
                Spacing = 4,
                HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center,
                VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center,
            };

            UpdateStyle(style);
        }

        public void AddWidget<T>(Component<T> component)
            where T : Widget
        {
            UiWidget.Widgets.Add(component.UiWidget);
        }
    }
}
