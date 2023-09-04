using FpsGame.Common.ClientData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Messages
{
    public abstract class MessageProcessor
    {
        protected Type type;

        public MessageProcessor(Type type) 
        {
            this.type = type;
        }

        public abstract void ProcessMessage(JObject message);
    }
}
