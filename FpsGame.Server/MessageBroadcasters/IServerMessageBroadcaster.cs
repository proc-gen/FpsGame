using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageBroadcasters
{
    public interface IServerMessageBroadcaster
    {
        void Broadcast(int serverTick);
    }
}
