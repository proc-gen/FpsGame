using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public struct ClientDisconnect : IMessageType
    {
        public ClientDisconnect()
        {
            Type = GetType().Name;
        }

        public string Type { get; set; }

        public string MessageType()
        {
            return Type;
        }
    }
}
