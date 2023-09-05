using Arch.Core.Extensions;
using FpsGame.Common.ClientData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageProcessors
{
    public class ClientDisconnectProcessor : IServerMessageProcessor
    {
        public ClientDisconnectProcessor() 
        { 
        }

        public void ProcessMessage(ClientData.ClientData message)
        {
            message.Client.Disconnect();
        }
    }
}
