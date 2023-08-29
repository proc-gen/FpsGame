using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public class PlayersInfo : IMessageType
    {
        public PlayersInfo() 
        {
            Type = GetType().Name;
            Players = new List<PlayerInfo>();
        }

        public string Type { get; set; }
        public List<PlayerInfo> Players { get; set; }
        public string MessageType()
        {
            return Type;
        }
    }

    public struct PlayerInfo
    {
        public string Name { get; set; }
        public Vector3 Color { get; set; }
        public long Ping { get; set; }
    }
}
