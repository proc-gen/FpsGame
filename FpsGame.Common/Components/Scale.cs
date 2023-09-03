using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct Scale : ISerializableComponent
    {
        public SerializableObjectState ComponentState { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Scale()
        {
            ComponentState = SerializableObjectState.Add;
        }

        public Scale(float defaultScale = 1f)
        {
            X = Y = Z = defaultScale;
            ComponentState = SerializableObjectState.Add;
        }
    }
}
