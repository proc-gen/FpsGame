using FpsGame.Ui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using MyraTextBox = Myra.Graphics2D.UI.TextBox;

namespace FpsGame.Ui.Components
{
    public class TextBox : Component<MyraTextBox>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Styles.Thickness(4),
        };

        public string Text { get { return UiWidget.Text; } }

        public TextBox(string id) 
            : base(id)
        {
            Init(string.Empty, baseStyle);
        }

        public TextBox(string id, string text)
            : base(id)
        {
            Init(text, baseStyle);
        }

        public TextBox(string id, string text, Style style)
            : base(id)
        {
            Init(text, style);
        }

        private void Init(string text, Style style)
        {
            UiWidget = new MyraTextBox()
            {
                Id = Id,
                Text = text,
            };

            UpdateStyle(style);
        }
    }
}
