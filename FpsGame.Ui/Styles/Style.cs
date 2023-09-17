using FontStashSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Ui.Styles
{
    public struct Style
    {
        public int? Width;
        public int? Height;
        public Thickness? Padding;
        public Thickness? Margin;
        public HorizontalAlignment? HorizontalAlignment;
        public VerticalAlignment? VerticalAlignment;
        public Vector2? FontScale;
    }
}
