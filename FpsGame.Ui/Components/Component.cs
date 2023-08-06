using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Ui.Components
{
    public abstract class Component<T>
        where T: Widget
    {
        public T UiWidget { get; protected set; }
    }
}
