using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server.Systems;
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

        private readonly JsonNetSerializer serializer = new JsonNetSerializer();
        private readonly Dictionary<Type, Converter> converters;

        World world;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;
        List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();

        const int SendRate = 15;
        private int serverTick = 0;

        public Server()
        {
            listener = new TcpListener(IPAddress.Loopback, 1234);
            listener.Start();

            world = World.Create();

            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f), new ModelRotator() { XIncrement = 1/500f, YIncrement = 1/500f});
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 2, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = -2, Y = 0, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = 2, Z = 0 }, new Rotation(), new Scale(0.5f));
            world.Create(new RenderModel() { Model = "cube" }, new Position() { X = 0, Y = -2, Z = 0 }, new Rotation(), new Scale(0.5f));

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.ModelRotator, new QueryDescription().WithAll<Rotation, ModelRotator>() },
            };

            converters = new Dictionary<Type, Converter>()
            {
                {typeof(RenderModel), new RenderModelConverter()},
                {typeof(Position), new PositionConverter()},
                {typeof(Rotation), new RotationConverter()},
                {typeof(Scale), new ScaleConverter()},
                {typeof(ModelRotator), new ModelRotatorConverter()},
            };

            updateSystems.Add(new ModelRotatorSystem(world, queryDescriptions));
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Run(cancellationToken);
            }
        }

        private async void AddNewClients(CancellationToken cancellationToken)
        {
            try { 
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                var client = new ServerSideClient(tcpClient);
                client.BeginReceiving(cancellationToken);
                client.Disconnected += ClientDisconnected;
                newClients.Add(client);
            }
            catch (Exception ex)
            {

            }
        }

        private void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var system in updateSystems)
                {
                    system.Update();
                }

                AddNewClients(cancellationToken);

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
                var data = serializer.Serialize(SerializableWorld.SerializeWorld(world, false));
                foreach (var client in clients)
                {
                    client.Send(data);
                }
            }

            if (newClients.Any())
            {
                var fullData = serializer.Serialize(SerializableWorld.SerializeWorld(world, true));
                foreach(var client in newClients)
                {
                    clients.Add(client);
                    client.Send(fullData);
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
