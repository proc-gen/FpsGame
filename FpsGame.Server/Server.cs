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
    public class Server : IDisposable
    {
        private bool disposedValue;
        private readonly List<ServerSideClient> clients = new List<ServerSideClient>();
        private readonly List<ServerSideClient> newClients = new List<ServerSideClient>();
        private readonly TcpListener listener;

        const int SendRate = 15;
        private int serverTick = 0;

        public Server()
        {
            listener = new TcpListener(IPAddress.Loopback, 1234);
            listener.Start();
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            AddNewClients(cancellationToken);
            Task.Run(() => Run(cancellationToken));
        }

        private async void AddNewClients(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                var client = new ServerSideClient(tcpClient);
                client.BeginReceiving(cancellationToken);
                client.Disconnected += ClientDisconnected;
                newClients.Add(client);
            }
        }

        private void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // TODO: Do game loop things

                if(serverTick++ % (60 / SendRate) == 0)
                {
                    BroadcastWorld();
                }
            }
        }

        private void ClientDisconnected(object sender, EventArgs e)
        {
            if (sender is ServerSideClient serverSideClient)
            {
                clients.Remove(serverSideClient);
            }
        }
    
        private void BroadcastWorld()
        {
            if (clients.Any())
            {
                foreach(var client in clients)
                {
                    // TODO: Send world data
                }
            }

            if (newClients.Any())
            {
                foreach(var client in newClients)
                {
                    clients.Add(client);
                    // TODO: Send full serialization data
                }
                newClients.Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    listener.Stop();

                    if (newClients.Any())
                    {
                        foreach (var client in newClients)
                        {
                            client.Dispose();
                        }
                    }

                    if (clients.Any() )
                    {
                        foreach(var client in clients)
                        {
                            client.Dispose();
                        }
                    }
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
