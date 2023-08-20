using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.ClientData
{
    public class ClientData
    {
        public ServerSideClient Client { get; set; }
        public EntityReference EntityReference { get; set; }
        public string Data { get; set; }
    }
}
