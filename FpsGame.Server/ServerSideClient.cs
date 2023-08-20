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
    public class ServerSideClient : IDisposable
    {
        private bool disposedValue;
        private readonly TcpClient client;
        private readonly NetworkStream networkStream;
        private BinaryReader reader;

        public event EventHandler Disconnected;
        Func<EntityReference, string, bool> AddDataToProcess;
        public EntityReference entityReference { get; private set; }

        public ServerSideClient(TcpClient client, Func<EntityReference, string, bool> addDataToProcess)
        {
            this.client = client;
            networkStream = client.GetStream();
            reader = new BinaryReader(networkStream);
            AddDataToProcess = addDataToProcess;
        }

        public void SetEntityReference(EntityReference entityReference)
        {
            this.entityReference = entityReference;
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
                        AddDataToProcess(entityReference, reader.ReadString());
                    }
                }
                catch
                {
                    Disconnect();
                    return;
                }
            }
        }

        private void Disconnect()
        {
            client?.Close();
            Disconnected?.Invoke(this, EventArgs.Empty);
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
