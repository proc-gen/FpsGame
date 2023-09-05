using Arch.Core.Extensions;
using FpsGame.Common.ClientData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageProcessors
{
    public class ClientInputProcessor : IServerMessageProcessor
    {
        public ClientInputProcessor()
        {
        }

        public void ProcessMessage(ClientData.ClientData message)
        {
            message.EntityReference.Entity.Set(message.Data.ToObject<ClientInput>());
        }
    }
}
