using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Graphics2D.UI;
using MyraPanel = Myra.Graphics2D.UI.Panel;

namespace FpsGame.Ui.Components
{
    public class Panel : Component<MyraPanel>
    {
        private static Style baseStyle = new Style()
        {

        };

        public Panel(string id)
            : base(id)
        {
            Init(baseStyle);
        }

        public Panel(string id, Style style)
            : base(id)
        {
            Init(style);
        }

        private void Init(Style style)
        {
            UiWidget = new MyraPanel()
            {
                Id = Id,
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
