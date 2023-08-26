using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using Myra.Graphics2D.UI;
using MyraLabel = Myra.Graphics2D.UI.Label;

namespace FpsGame.Ui.Components
{
    public class Label : Component<MyraLabel>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Styles.Thickness(4),
        };

        public string Text { get; private set; }

        public Label(string id, string text)
            : base(id)
        {
            Init(text, baseStyle);
        }

        public Label(string id, string text, Style style)
            : base(id)
        {
            Init(text, style);
        }

        private void Init(string text, Style style)
        {
            Text = text;

            UiWidget = new MyraLabel()
            {
                Id = Id,
                Text = text,
            };

            UpdateStyle(style);
        }

        public void UpdateText(string text)
        {
            Text = text;
            UiWidget.Text = text;
        }
    }
}
