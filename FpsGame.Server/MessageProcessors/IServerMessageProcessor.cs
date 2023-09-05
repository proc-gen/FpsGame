using FpsGame.Common.ClientData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageProcessors
{
    public interface IServerMessageProcessor
    {
        void ProcessMessage(ClientData.ClientData message);
    }
}
