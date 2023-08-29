using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.ClientData;
using FpsGame.Common.Components;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FpsGame.Server
{
    public enum ServerSideClientStatus
    {
        Connected,
        JoinedGame,
        InGame,
        Disconnected,
        Removed,
    }

    public class ServerSideClient : IDisposable
    {
        private bool disposedValue;
        private TcpClient client;

        MessageSerializer messageSerializer;

        Func<ServerSideClient, JObject, bool> AddDataToProcess;
        public EntityReference entityReference { get; private set; }
        public ServerSideClientStatus Status { get; set; }

        IPEndPoint remoteIpEndPoint;
        PingHelper pingHelper = new PingHelper();

        public ServerSideClient(TcpClient client, Func<ServerSideClient, JObject, bool> addDataToProcess)
        {
            this.client = client;
            AddDataToProcess = addDataToProcess;
            messageSerializer = new MessageSerializer(client.GetStream(), addDataFromMessage);
            remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Status = ServerSideClientStatus.Connected;
        }

        private bool addDataFromMessage(JObject data)
        {
            return AddDataToProcess(this, data);
        }

        public void SetEntityReference(EntityReference entityReference)
        {
            if(Status == ServerSideClientStatus.Connected)
            {
                this.entityReference = entityReference;
                Status = ServerSideClientStatus.JoinedGame;
            }
        }

        public uint GetPlayerId()
        {
            return entityReference.Entity.Get<Player>().Id;
        }

        public void BeginReceiving(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                messageSerializer.Receive();
            }

            return;
        }

        public long CheckPing()
        {
            pingHelper.CheckPing(remoteIpEndPoint.Address.ToString());
            return pingHelper.PingMs;
        }

        public void Disconnect()
        {
            Status = ServerSideClientStatus.Disconnected;
            client?.Close();
        }

        public void Send(string data)
        {
            messageSerializer.Send(data);
        }

        public void Send(IMessageType data)
        {
            messageSerializer.Send(data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageSerializer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
