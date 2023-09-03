using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct Player : ISerializableComponent
    {
        public SerializableObjectState ComponentState { get; set; }
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Color { get; set; }

        public Player()
        {
            ComponentState = SerializableObjectState.Add;
        }
    }
}
