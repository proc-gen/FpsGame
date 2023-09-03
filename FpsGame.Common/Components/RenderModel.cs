using FpsGame.Common.Constants;
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
        public SerializableObjectState ComponentState { get; set; }
        public string Model { get; set; }

        public RenderModel()
        {
            ComponentState = SerializableObjectState.Add;
        }
    }
}
