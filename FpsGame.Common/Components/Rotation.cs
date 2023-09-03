using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct Rotation : ISerializableComponent
    {
        public SerializableObjectState ComponentState { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Rotation()
        {
            ComponentState = SerializableObjectState.Add;
        }
    }
}
