using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FpsGame.Server
{
    public class Client : IDisposable
    {
        private bool disposedValue;

        private readonly TcpClient client;
        private NetworkStream stream;
        private BinaryReader reader;
        Func<string, bool> AddDataToProcess;

        public Client(Func<string, bool> addDataToProcess)
        {
            client = new TcpClient();
            AddDataToProcess = addDataToProcess;
        }

        public async Task Join(CancellationToken cancellationToken)
        {
            await client.ConnectAsync(IPAddress.Loopback, 1234);
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            BeginReceiving(cancellationToken);
        }

        public async void BeginReceiving(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        AddDataToProcess(reader.ReadString());
                    }
                }
            }
            catch (Exception ex)
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
                    stream?.Dispose();
                    client?.Dispose();
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
