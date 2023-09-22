using FpsGame.Ui.Styles;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyraLabel = Myra.Graphics2D.UI.Label;

namespace FpsGame.Ui.Components
{
    public abstract class Component<T>
        where T: Widget
    {
        public T UiWidget { get; protected set; }
        public string Id { get; private set; }

        public Component(string id)
        {
            Id = id;
        }

        public void UpdateStyle(Style style)
        {
            if (UiWidget != null)
            {
                if (style.Width.HasValue)
                {
                    UiWidget.Width = style.Width.Value;
                }

                if (style.Height.HasValue)
                {
                    UiWidget.Height = style.Height.Value;
                }

                if (style.Margin.HasValue)
                {
                    UiWidget.Margin = new Myra.Graphics2D.Thickness(style.Margin.Value.Left, style.Margin.Value.Top, style.Margin.Value.Right, style.Margin.Value.Bottom);
                }

                if (style.Padding.HasValue)
                {
                    UiWidget.Padding = new Myra.Graphics2D.Thickness(style.Padding.Value.Left, style.Padding.Value.Top, style.Padding.Value.Right, style.Padding.Value.Bottom);
                }

                if(style.FontScale.HasValue)
                {
                    UiWidget.Scale = style.FontScale.Value;
                }

                if(style.HorizontalAlignment.HasValue)
                {
                    UiWidget.HorizontalAlignment = style.HorizontalAlignment.Value.GetMyraHorizontalAlignment();
                }

                if (style.VerticalAlignment.HasValue)
                {
                    UiWidget.VerticalAlignment = style.VerticalAlignment.Value.GetMyraVerticalAlignment();
                }
            }
        }
    }
}
