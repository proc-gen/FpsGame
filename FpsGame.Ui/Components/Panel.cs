using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra.Graphics2D.UI;
using MyraPanel = Myra.Graphics2D.UI.Panel;

namespace FpsGame.Ui.Components
{
    public class Panel : Component<MyraPanel>
    {
        public Panel(string id)
            : base(id)
        { 
            UiWidget = new MyraPanel()
            {
                Id = id,
            };
        }

        public void AddWidget<T>(Component<T> component)
            where T : Widget
        {
            UiWidget.Widgets.Add(component.UiWidget);
        }
    }
}
