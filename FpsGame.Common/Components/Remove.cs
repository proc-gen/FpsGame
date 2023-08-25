using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct Remove : ISerializableComponent
    {
        public SerializableObjectState ComponentState { get; set; }
    }
}
