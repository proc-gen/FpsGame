using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Constants
{
    public enum MessageType
    {
        Unknown,
        JoinGame,
        RejoinGame,
        WorldFull,
        WorldUpdate,
        LeaveGame,
    }
}
