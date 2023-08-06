using System;
using System.Collections.Generic;
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
            networkStream?.Close();
            networkStream?.Dispose();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Dispose();  
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
