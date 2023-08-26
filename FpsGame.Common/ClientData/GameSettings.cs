using FpsGame.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public class GameSettings : IMessageType
    {
        public GameSettings()
        {
            Type = GetType().Name;
        }

        public string Type { get; set; }
        public GameMode GameMode { get; set; }
        public string GameName { get; set; }
        public List<IPAddress> GameIPAddress { get; set; }
        public int GamePort { get; set; }

        public string MessageType()
        {
            return Type;
        }
    }
}
