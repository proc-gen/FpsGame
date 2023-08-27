using FpsGame.Common.Constants;
using System.Collections.Generic;
using System.Net;

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
        public IPAddress GameIPAddress { get; set; }
        public int GamePort { get; set; }

        public string MessageType()
        {
            return Type;
        }
    }
}
