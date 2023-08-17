

using Microsoft.Xna.Framework;

namespace FpsGame.Server.ClientData
{
    public struct ClientInput
    {
        public bool Forward { get; set; }
        public bool Backward { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public Vector2 MouseDelta { get; set; }
    }
}
