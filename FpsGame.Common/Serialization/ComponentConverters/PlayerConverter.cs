using FpsGame.Common.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.ComponentConverters
{
    public class PlayerConverter : JObjectConverter
    {
        public override object Convert(object data)
        {
            return Convert<Player>((JObject)data);
        }
    }
}
