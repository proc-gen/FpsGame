using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.Utils
{
    public class PingHelper
    {
        Ping serverPing = new Ping();
        public long PingMs { get; private set; } = -1;
        Task pingTask;

        public void CheckPing(string address)
        {
            if (pingTask == null)
            {
                pingTask = Task.Run(async () =>
                {
                    var ping = await serverPing.SendPingAsync(address);
                    PingMs = ping.RoundtripTime;
                    pingTask = null;
                });
            }
        }
    }
}
