using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.ComponentConverters
{
    public abstract class Converter
    {
        public Converter() { }

        public abstract object Convert(object data);
    }
}
