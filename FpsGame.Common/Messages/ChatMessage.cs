using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public struct ChatMessage : IMessageType
    {
        public ChatMessage() 
        {
            Type = GetType().Name;
        }

        public string Type { get; set; }

        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }

        public string MessageType()
        {
            return Type;
        }
    }
}
