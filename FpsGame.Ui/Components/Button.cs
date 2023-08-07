using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using MyraButton = Myra.Graphics2D.UI.TextButton;

namespace FpsGame.Ui.Components
{
    public class Button : Component<MyraButton>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Styles.Thickness(4),
        };

        public string Text { get; set; }

        public Button(string id, string text, EventHandler onClick)
            : base(id)
        {
            Init(text, onClick, baseStyle);
        }

        public Button(string id, string text, EventHandler onClick, Style style)
            : base(id)
        {
            Init(text, onClick, style);
        }

        private void Init(string text, EventHandler onClick, Style style) 
        {
            Text = text;

            UiWidget = new MyraButton()
            {
                Id = Id,
                Text = text,
            };

            UiWidget.Click += onClick;

            UpdateStyle(style);
        }
    }
}
