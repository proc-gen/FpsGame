using FpsGame.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Containers
{
    public struct GameSettings
    {
        public GameMode GameMode { get; set; }
        public string GameName { get; set; }
        public List<IPAddress> GameIPAddress { get; set; }
        public int GamePort { get; set; }
    }
}
