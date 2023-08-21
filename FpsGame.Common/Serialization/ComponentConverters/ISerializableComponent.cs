using FpsGame.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.ComponentConverters
{
    internal interface ISerializableComponent
    {
        public SerializableObjectState ComponentState { get; set; }
    }
}
