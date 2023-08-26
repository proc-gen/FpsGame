using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private NetworkStream networkStream;
        private BinaryReader reader;

        MessageSerializer messageSerializer;

        Func<ServerSideClient, JObject, bool> AddDataToProcess;
        public EntityReference entityReference { get; private set; }
        public ServerSideClientStatus Status { get; set; }

        public ServerSideClient(TcpClient client, Func<ServerSideClient, JObject, bool> addDataToProcess)
        {
            this.client = client;
            networkStream = client.GetStream();
            messageSerializer = new MessageSerializer(networkStream);
            reader = new BinaryReader(networkStream);
            AddDataToProcess = addDataToProcess;
            Status = ServerSideClientStatus.Connected;
        }

        public void Reset(TcpClient client)
        {
            this.client = client;
            networkStream = client.GetStream();
            reader = new BinaryReader(networkStream);
            Status = ServerSideClientStatus.Connected;
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
                try
                {
                    if (networkStream.DataAvailable)
                    {
                        string message = reader.ReadString();
                        if (!string.IsNullOrEmpty(message))
                        {
                            using (var sr = new StringReader(message))
                            {
                                using (JsonReader reader = new JsonTextReader(sr))
                                {
                                    JsonSerializer serializer = new JsonSerializer();

                                    AddDataToProcess(this, serializer.Deserialize<JObject>(reader));
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Disconnect();
                    return;
                }
            }
        }

        public void Disconnect()
        {
            Status = ServerSideClientStatus.Disconnected;
            client?.Close();
            reader?.Close();
            networkStream?.Close();
        }

        public void Send(string data)
        {
            messageSerializer.Send(data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader?.Dispose();
                    networkStream?.Dispose();
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
