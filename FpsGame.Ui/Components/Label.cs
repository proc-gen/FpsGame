using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra.Graphics2D.UI;
using MyraLabel = Myra.Graphics2D.UI.Label;

namespace FpsGame.Ui.Components
{
    public class Label : Component<MyraLabel>
    {
        public string Id { get; private set; }
        public string Text { get; set; }

        public Label(string id, string text) 
        { 
            Id = id;
            Text = text;

            UiWidget = new MyraLabel()
            {
                Id = id,
                Text = text,
            };
        }
    }
}
