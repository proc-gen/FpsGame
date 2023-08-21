using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Containers;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Server.ClientData;
using FpsGame.Server.Systems;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly List<TcpListener> listeners = new List<TcpListener>();

        private readonly JsonNetSerializer serializer = new JsonNetSerializer();

        World world;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;
        List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();
        Queue<ClientData.ClientData> ClientMessages = new Queue<ClientData.ClientData>();
        
        List<Task> tasks = new List<Task>();
        CancellationToken cancellationToken;

        GameSettings gameSettings;

        const int SendRate = 60;
        private int serverTick = 0;

        public Server(CancellationToken cancellationToken, GameSettings gameSettings)
        {
            this.cancellationToken = cancellationToken;
            this.gameSettings = gameSettings;

            foreach(var ip in gameSettings.GameIPAddress)
            {
                var listener = new TcpListener(ip, gameSettings.GamePort);
                listener.Start();
                listeners.Add(listener);
            }

            world = World.Create();

            Random random = new Random();

            for(int i = 0; i < 100; i++)
            {
                world.Create(
                    new RenderModel() { Model = "cube" }, 
                    new Position() { X = (float)random.NextDouble() * 100f - 50f, Y = -5f, Z = (float)random.NextDouble() * 100f - 50f }, 
                    new Rotation(), 
                    new Scale(0.5f + (float)random.NextDouble()), 
                    new ModelRotator() { XIncrement = (1 + (float)random.NextDouble()) / 500f, YIncrement = (1 + (float)random.NextDouble()) / 500f, ZIncrement = (1 + (float)random.NextDouble()) / 500f }
                );
            }

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.ModelRotator, new QueryDescription().WithAll<Rotation, ModelRotator>() },
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Camera, ClientInput>() },
            };

            updateSystems.Add(new PlayerInputSystem(world, queryDescriptions));
            updateSystems.Add(new ModelRotatorSystem(world, queryDescriptions));
        }

        private async void AddNewClients()
        {
            try {
                foreach (var listener in listeners)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                    var client = new ServerSideClient(tcpClient, AddDataToProcess);

                    client.Disconnected += ClientDisconnected;
                    tasks.Add(Task.Run(() => client.BeginReceiving(cancellationToken), cancellationToken));
                    newClients.Add(client);
                }
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

                                JObject clientInput = serializer.Deserialize<JObject>(reader);

                                if (clientInput.ContainsKey("Name"))
                                {
                                    var playerSettings = clientInput.ToObject<PlayerSettings>();

                                    var entity = world.Create(new Player() { Id = (uint)clients.Count + 1, Name = playerSettings.Name, Color = playerSettings.Color }, new Camera(), new ClientInput(), new RenderModel() { Model = "sphere" });
                                    message.Client.SetEntityReference(entity.Reference());
                                }
                                else if (clientInput.ContainsKey("Forward"))
                                {
                                    message.EntityReference.Entity.Set(clientInput.ToObject<ClientInput>());
                                }
                            }
                        }
                    }
                }

                foreach (var system in updateSystems)
                {
                    system.Update(gameTime);
                }

                if (gameSettings.GameMode != GameMode.SinglePlayer || !clients.Any())
                {
                    AddNewClients();
                }

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
                var dataToSerialize = SerializableWorld.SerializeWorld(world, false);
                if (newClients.Where(a => a.Joined).Any())
                {
                    foreach(var client in newClients.Where(a => a.Joined))
                    {
                        dataToSerialize.Entities.Add(SerializableEntity.SerializeEntity(client.entityReference.Entity, client.entityReference.Entity.GetAllComponents(), true));
                    }
                }
                var data = serializer.Serialize(dataToSerialize);
                foreach (var client in clients)
                {
                    client.Send(data);
                }
            }

            if (newClients.Where(a => a.Joined).Any())
            {
                foreach(var client in newClients.Where(a => a.Joined))
                {
                    var serializedWorld = SerializableWorld.SerializeWorld(world, true);
                    serializedWorld.PlayerId = client.GetPlayerId();
                    clients.Add(client);
                    client.Send(serializer.Serialize(serializedWorld));
                }
                newClients.RemoveAll(a => a.Joined);
            }
        }

        public bool AddDataToProcess(ServerSideClient client, string data)
        {
            ClientMessages.Enqueue(new ClientData.ClientData() { Client = client, EntityReference = client.entityReference, Data = data});
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (listeners.Any())
                    {
                        foreach(var listener in listeners)
                        {
                            listener.Stop();
                        }
                    }
                    
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
