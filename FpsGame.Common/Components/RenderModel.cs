using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct RenderModel : ISerializableComponent
    {
        public bool IsChanged { get; set; }
        public string Model { get; set; }
    }
}
