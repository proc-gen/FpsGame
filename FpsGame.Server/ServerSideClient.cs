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
        private readonly byte[] buffer = new byte[512];
        private readonly TcpClient client;
        private readonly NetworkStream networkStream;

        public event EventHandler Disconnected;

        public ServerSideClient(TcpClient client)
        {
            this.client = client;
            networkStream = client.GetStream();
        }

        public async void BeginReceiving(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int length = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (length == 0)
                    {
                        Disconnect();
                        return;
                    }

                    // TODO: Process incoming data
                }
                catch
                {
                    Disconnect();
                    return;
                }
            }

            Disconnect();
        }

        private void Disconnect()
        {
            client?.Close();
            Disconnected?.Invoke(this, EventArgs.Empty);
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
