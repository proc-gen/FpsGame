using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra.Graphics2D.UI;
using MyraButton = Myra.Graphics2D.UI.TextButton;

namespace FpsGame.Ui.Components
{
    public class Button : Component<MyraButton>
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public Button(string id, string text, EventHandler onClick)
        {
            Id = id;
            Text = text;
            UiWidget = new MyraButton()
            {
                Id = id,
                Text = text,
            };

            UiWidget.Click += onClick;
        }
    }
}
