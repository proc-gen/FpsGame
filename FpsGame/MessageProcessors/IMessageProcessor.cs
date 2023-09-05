using FpsGame.Common.ClientData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.MessageProcessors
{
    public interface IMessageProcessor
    {
        void ProcessMessage(JObject data);
    }
}
