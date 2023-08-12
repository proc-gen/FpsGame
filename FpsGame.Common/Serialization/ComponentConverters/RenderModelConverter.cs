using FpsGame.Common.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.ComponentConverters
{
    public class RenderModelConverter : JObjectConverter
    {
        public override object Convert(object data)
        {
            return Convert<RenderModel>((JObject)data);
        }
    }
}
