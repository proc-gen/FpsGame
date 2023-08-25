using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.ClientData
{
    public struct PlayerSettings : IClientDataType
    {
        public PlayerSettings()
        {
            Type = GetType().Name;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public Vector3 Color { get; set; }

        public string ClientDataType()
        {
            return Type;
        }
    }
}
