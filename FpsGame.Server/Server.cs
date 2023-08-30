using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Ecs.Systems;
using FpsGame.Common.Serialization;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization.Serializers;
using FpsGame.Common.ClientData;
using FpsGame.Server.Systems;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Arch.Core.Events;

namespace FpsGame.Server
{
    public class Server : IDisposable
    {
        private bool disposedValue;
        private readonly List<ServerSideClient> clients = new List<ServerSideClient>();
        private readonly List<ServerSideClient> newClients = new List<ServerSideClient>();
        private readonly TcpListener listener;

        private readonly JsonNetArchSerializer serializer = new JsonNetArchSerializer();

        World world;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;
        List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();
        Queue<ClientData.ClientData> ClientMessages = new Queue<ClientData.ClientData>();
        
        List<Task> tasks = new List<Task>();
        CancellationToken cancellationToken;

        GameSettings gameSettings;

        List<ChatMessage> messagesToSend = new List<ChatMessage>();
        Func<List<ChatMessage>, bool> sendMessages;

        const int SendRate = 60;
        private int serverTick = 0;

        public Server(CancellationToken cancellationToken, GameSettings gameSettings, Func<List<ChatMessage>, bool> sendMessages = null)
        {
            this.cancellationToken = cancellationToken;
            this.gameSettings = gameSettings;
            this.sendMessages = sendMessages;

            var ip = gameSettings.GameIPAddress;
            
            listener = new TcpListener(ip, gameSettings.GamePort);
            listener.Start();
            

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
            try
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);

                var client = new ServerSideClient(tcpClient, AddDataToProcess);

                tasks.Add(Task.Run(() => client.BeginReceiving(cancellationToken), cancellationToken));
                newClients.Add(client);
            }
            catch (OperationCanceledException ex)
            {
                //Do nothing
            }
        }

        public void Run(GameTime gameTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            HandleClientMessages();

            foreach (var system in updateSystems)
            {
                system.Update(gameTime);
            }

            if (gameSettings.GameMode != GameMode.SinglePlayer || !clients.Any())
            {
                AddNewClients();
            }

            serverTick++;

            if (serverTick % (60 / SendRate) == 0)
            {
                BroadcastWorld();
            }
            if(serverTick % 60 == 0)
            {
                BroadcastMessages();
                BroadcastPlayerInfo();
            }
        }

        private void HandleClientMessages()
        {
            if (ClientMessages.Count > 0)
            {
                do
                {
                    var message = ClientMessages.Dequeue();
                    if (message != null)
                    {
                        switch (message.Data["Type"].ToString())
                        {
                            case "PlayerSettings":
                                var playerSettings = message.Data.ToObject<PlayerSettings>();

                                var entity = world.Create(new Player() { Id = (uint)clients.Count + 1, Name = playerSettings.Name, Color = playerSettings.Color }, new Camera(), new ClientInput(), new RenderModel() { Model = "sphere" });
                                message.Client.SetEntityReference(entity.Reference());
                                messagesToSend.Add(new ChatMessage()
                                {
                                    SenderName = "Server",
                                    Message = string.Format("{0} has connected", playerSettings.Name),
                                    Time = DateTime.Now,
                                });
                                break;
                            case "ClientInput":
                                message.EntityReference.Entity.Set(message.Data.ToObject<ClientInput>());
                                break;
                            case "ClientDisconnect":
                                message.Client.Disconnect();
                                break;
                        }
                    }
                } while (ClientMessages.Count > 0);
            }
        }
    
        private void BroadcastWorld()
        {
            if (clients.Where(a => a.Status == ServerSideClientStatus.Disconnected).Any())
            {
                foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.Disconnected))
                {
                    client.entityReference.Entity.Add(new Remove());
                }
            }

            if (clients.Where(a => a.Status == ServerSideClientStatus.InGame).Any())
            {
                var dataToSerialize = SerializableWorld.SerializeWorld(world, false);
                if (newClients.Where(a => a.Status == ServerSideClientStatus.JoinedGame).Any())
                {
                    foreach (var client in newClients.Where(a => a.Status == ServerSideClientStatus.JoinedGame))
                    {
                        dataToSerialize.Entities.Add(SerializableEntity.SerializeEntity(client.entityReference.Entity, client.entityReference.Entity.GetAllComponents(), true));
                    }
                }
                var data = serializer.Serialize(dataToSerialize);
                foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
                {
                    client.Send(data);
                }
            }

            if (newClients.Where(a => a.Status == ServerSideClientStatus.JoinedGame).Any())
            {
                foreach (var client in newClients.Where(a => a.Status == ServerSideClientStatus.JoinedGame))
                {
                    var serializedWorld = SerializableWorld.SerializeWorld(world, true);
                    serializedWorld.PlayerId = client.GetPlayerId();
                    client.Send(new GameSettings() { GameName = gameSettings.GameName });
                    client.Send(serializer.Serialize(serializedWorld));
                    client.Status = ServerSideClientStatus.InGame;

                    clients.Add(client);
                }
                newClients.RemoveAll(a => a.Status == ServerSideClientStatus.JoinedGame);
            }

            if (clients.Where(a => a.Status == ServerSideClientStatus.Disconnected).Any())
            {
                foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.Disconnected))
                {
                    var playerInfo = client.entityReference.Entity.Get<Player>();
                    messagesToSend.Add(new ChatMessage()
                    {
                        SenderName = "Server",
                        Message = string.Format("{0} has disconnected", playerInfo.Name),
                        Time = DateTime.Now,
                    });
                    world.Destroy(client.entityReference.Entity);
                    client.SetEntityReference(EntityReference.Null);
                    client.Status = ServerSideClientStatus.Removed;
                }
            }
        }

        private void BroadcastMessages()
        {
            if(messagesToSend.Count > 0)
            {
                foreach(var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
                {
                    foreach(var message in messagesToSend)
                    {
                        client.Send(message);
                    }
                }

                if (sendMessages != null)
                {
                    sendMessages(messagesToSend);
                }

                messagesToSend.Clear();
            }
        }

        private void BroadcastPlayerInfo()
        {
            var PlayersInfo = new PlayersInfo();
            foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
            {
                client.CheckPing();
                var player = client.entityReference.Entity.Get<Player>();
                PlayersInfo.Players.Add(new PlayerInfo()
                {
                    Name = player.Name,
                    Color = player.Color,
                    Ping = client.CheckPing(),
                });
            }

            foreach (var client in clients.Where(a => a.Status == ServerSideClientStatus.InGame))
            {
                client.Send(PlayersInfo);
            }
        }

        public bool AddDataToProcess(ServerSideClient client, JObject data)
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
                    if (tasks.Any())
                    {
                        foreach (var task in tasks)
                        {
                            if (task.IsCompleted)
                            {
                                task.Dispose();
                            }
                        }
                    }
                    
                    listener.Stop();
                    

                    if (clients.Any())
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
