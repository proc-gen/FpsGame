using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server.ClientData;
using FpsGame.Server.Systems;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        World world;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;
        List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();
        Queue<ClientData.ClientData> ClientMessages = new Queue<ClientData.ClientData>();
        
        List<Task> tasks = new List<Task>();
        CancellationToken cancellationToken;

        const int SendRate = 60;
        private int serverTick = 0;

        public Server(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            listener = new TcpListener(IPAddress.Loopback, 1234);
            listener.Start();

            world = World.Create();

            Random random = new Random();

            for(int i = 0; i < 100; i++)
            {
                world.Create(
                    new RenderModel() { Model = "cube" }, 
                    new Position() { X = (float)random.NextDouble() * 100f - 50f, Y = (float)random.NextDouble() * 100f - 50f, Z = (float)random.NextDouble() * 100f - 50f }, 
                    new Rotation(), 
                    new Scale(0.5f + (float)random.NextDouble()), 
                    new ModelRotator() { XIncrement = (1 + (float)random.NextDouble()) / 500f, YIncrement = (1 + (float)random.NextDouble()) / 500f, ZIncrement = (1 + (float)random.NextDouble()) / 500f }
                );
            }

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.ModelRotator, new QueryDescription().WithAll<Rotation, ModelRotator>() },
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Position, ClientInput>() },
            };

            updateSystems.Add(new PlayerInputSystem(world, queryDescriptions));
            updateSystems.Add(new ModelRotatorSystem(world, queryDescriptions));
        }

        private async void AddNewClients()
        {
            try {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                var client = new ServerSideClient(tcpClient, AddDataToProcess);
                var entity = world.Create(new Player() { Id = (uint)clients.Count }, new Position() { X = 0, Y = 0, Z = 0 }, new Rotation(), new ClientInput());
                client.SetEntityReference(entity.Reference());
                client.Disconnected += ClientDisconnected;
                tasks.Add(Task.Run(() => client.BeginReceiving(cancellationToken), cancellationToken));
                newClients.Add(client);
            }
            catch { }
        }

        public void Run(GameTime gameTime)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (ClientMessages.Count > 0)
                {
                    var message = ClientMessages.Dequeue();
                    if (message != null)
                    {
                        using (var sr = new StringReader(message.Data))
                        {
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();

                                var clientInput = serializer.Deserialize<ClientInput>(reader);

                                message.EntityReference.Entity.Set(clientInput);
                            }
                        }
                    }
                }

                foreach (var system in updateSystems)
                {
                    system.Update(gameTime);
                }

                AddNewClients();

                if (serverTick++ % (60 / SendRate) == 0)
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

        public bool AddDataToProcess(EntityReference entityReference, string data)
        {
            ClientMessages.Enqueue(new ClientData.ClientData() { EntityReference = entityReference, Data = data});
            return true;
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
