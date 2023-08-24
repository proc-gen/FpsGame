using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Ui.Styles
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        Stretch,
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Stretch,
    }

    public static class AlignmentExtensions
    {
        public static Myra.Graphics2D.UI.HorizontalAlignment GetMyraHorizontalAlignment(this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return Myra.Graphics2D.UI.HorizontalAlignment.Left;
                case HorizontalAlignment.Center:
                    return Myra.Graphics2D.UI.HorizontalAlignment.Center;
                case HorizontalAlignment.Right:
                    return Myra.Graphics2D.UI.HorizontalAlignment.Right;
                case HorizontalAlignment.Stretch:
                    return Myra.Graphics2D.UI.HorizontalAlignment.Stretch;
            }

            return Myra.Graphics2D.UI.HorizontalAlignment.Center;
        }

        public static Myra.Graphics2D.UI.VerticalAlignment GetMyraVerticalAlignment(this VerticalAlignment alignment)
        {
            switch (alignment)
            {
                case VerticalAlignment.Top:
                    return Myra.Graphics2D.UI.VerticalAlignment.Top;
                case VerticalAlignment.Center:
                    return Myra.Graphics2D.UI.VerticalAlignment.Center;
                case VerticalAlignment.Bottom:
                    return Myra.Graphics2D.UI.VerticalAlignment.Bottom;
                case VerticalAlignment.Stretch:
                    return Myra.Graphics2D.UI.VerticalAlignment.Stretch;
            }

            return Myra.Graphics2D.UI.VerticalAlignment.Center;
        }
    }
}
