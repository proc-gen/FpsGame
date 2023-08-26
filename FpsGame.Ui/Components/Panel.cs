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
            HorizontalAlignment = Styles.HorizontalAlignment.Center,
            VerticalAlignment = Styles.VerticalAlignment.Center,
        };

        public VerticalPanel(string id)
            : base(id)
        {
            Init(baseStyle);
        }

        public VerticalPanel(string id, Style style)
            : base(id)
        {
            Init(style);
        }

        private void Init(Style style)
        {
            UiWidget = new MyraPanel()
            {
                Id = Id,
                Spacing = 4,
                HorizontalAlignment = style.HorizontalAlignment.HasValue ? style.HorizontalAlignment.Value.GetMyraHorizontalAlignment() : baseStyle.HorizontalAlignment.Value.GetMyraHorizontalAlignment(),
                VerticalAlignment = style.VerticalAlignment.HasValue ? style.VerticalAlignment.Value.GetMyraVerticalAlignment() : baseStyle.VerticalAlignment.Value.GetMyraVerticalAlignment(),
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
