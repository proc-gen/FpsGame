using FpsGame.Common.ClientData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageBroadcasters
{
    public class ChatMessageBroadcaster : IServerMessageBroadcaster
    {
        List<ServerSideClient> clients;
        List<ChatMessage> messagesToSend;
        Func<object, bool> sendData;

        public ChatMessageBroadcaster(
            List<ServerSideClient> clients,
            List<ChatMessage> messagesToSend,
            Func<object, bool> sendData
        ) 
        {
            this.clients = clients;
            this.messagesToSend = messagesToSend;
            this.sendData = sendData;
        }

        public void Broadcast(int serverTick)
        {
            if (serverTick % 60 == 0)
            {
                if (messagesToSend.Count > 0)
                {
                    foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
                    {
                        foreach (var message in messagesToSend)
                        {
                            client.Send(message);
                        }
                    }

                    if (sendData != null)
                    {
                        sendData(messagesToSend);
                    }

                    messagesToSend.Clear();
                }
            }
        }
    }
}
