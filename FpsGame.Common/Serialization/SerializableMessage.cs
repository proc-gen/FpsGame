using FpsGame.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization
{
    public abstract class SerializableMessage
    {
        public virtual MessageType MessageType { get; set; }
    }
}
