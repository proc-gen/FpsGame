using Arch.Core;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.Serializers
{
    public abstract class ArchSerializer<S>
    {
        public abstract S Serialize(SerializableWorld data);

        public abstract void Deserialize(S data, SerializableWorld world);
    }
}
