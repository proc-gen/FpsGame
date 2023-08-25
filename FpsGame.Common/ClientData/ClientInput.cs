

using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace FpsGame.Common.ClientData
{
    public struct ClientInput : IClientDataType
    {
        public ClientInput() 
        { 
            Type = GetType().Name;
        }

        public string Type { get; set; }
        public bool Forward { get; set; }
        public bool Backward { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public Vector2 MouseDelta { get; set; }
        public Vector2 LeftStick { get; set; }
        public Vector2 RightStick { get; set;}

        public string ClientDataType()
        {
            return Type;
        }
    }
}
