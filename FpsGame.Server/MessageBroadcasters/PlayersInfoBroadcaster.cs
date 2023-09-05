using Arch.Core.Extensions;
using FpsGame.Common.ClientData;
using FpsGame.Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageBroadcasters
{
    public class PlayersInfoBroadcaster : IServerMessageBroadcaster
    {
        List<ServerSideClient> clients;
        Func<object, bool> sendData;

        public PlayersInfoBroadcaster(
            List<ServerSideClient> clients,
            Func<object, bool> sendData
        ) 
        {
            this.clients = clients;
            this.sendData = sendData;
        }

        public void Broadcast(int serverTick)
        {
            if (serverTick % 60 == 0)
            {
                var PlayersInfo = new PlayersInfo();
                foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
                {
                    client.CheckPing();
                    var player = client.entityReference.Entity.Get<Player>();
                    PlayersInfo.Players.Add(new PlayerInfo()
                    {
                        Name = player.Name,
                        Color = player.Color,
                        Ping = client.CheckPing(),
                    });
                }

                if (sendData != null)
                {
                    sendData(PlayersInfo);
                }
                foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
                {
                    client.Send(PlayersInfo);
                }
            }
        }
    }
}
