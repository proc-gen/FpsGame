using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Ui.Styles
{
    public struct Thickness
    {
        public int Top;
        public int Bottom;
        public int Left;
        public int Right;

        public Thickness(int thickness)
        {
            Top = Bottom = Left = Right = thickness;
        }

        public Thickness(int horizontal, int vertical)
        {
            Top = Bottom = vertical;
            Left = Right = horizontal;
        }
    }
}
