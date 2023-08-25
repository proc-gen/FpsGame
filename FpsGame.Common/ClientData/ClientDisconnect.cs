using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public struct ClientDisconnect : IClientDataType
    {
        public ClientDisconnect()
        {
            Type = GetType().Name;
        }

        public string Type { get; set; }

        public string ClientDataType()
        {
            return Type;
        }
    }
}
