using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
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

        Func<ServerSideClient, string, bool> AddDataToProcess;
        public EntityReference entityReference { get; private set; }
        public ServerSideClientStatus Status { get; set; }

        public ServerSideClient(TcpClient client, Func<ServerSideClient, string, bool> addDataToProcess)
        {
            this.client = client;
            networkStream = client.GetStream();
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
                        AddDataToProcess(this, reader.ReadString());
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
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write(data);
                    }

                    networkStream.Write(ms.ToArray());
                }
            }
            catch(Exception ex)
            {

            }
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
