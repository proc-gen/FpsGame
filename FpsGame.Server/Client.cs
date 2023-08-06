using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FpsGame.Server
{
    public class Client : IDisposable
    {
        private bool disposedValue;

        private readonly byte[] buffer = new byte[1024];
        private readonly TcpClient client;
        private NetworkStream stream;

        public Client()
        {
            client = new TcpClient();
        }

        public async Task Join(CancellationToken cancellationToken)
        {
            await client.ConnectAsync(IPAddress.Loopback, 1234);
            stream = client.GetStream();
            BeginReceiving(cancellationToken);
        }

        public async void BeginReceiving(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                int length = await stream.ReadAsync(buffer, 0, buffer.Length);

                if(length == 0)
                {
                    return;
                }

                // TODO: Process incoming data
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                    stream.Dispose();
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
