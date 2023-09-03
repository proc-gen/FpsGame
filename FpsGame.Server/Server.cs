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
using FpsGame.Common.Level;
using BepuUtilities.Memory;
using BepuUtilities;
using BepuPhysics;
using FpsGame.Common.Physics;
using BepuPhysics.Collidables;

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

        PhysicsWorld physicsWorld;
        
        List<Task> tasks = new List<Task>();
        Task addNewClientTask = null;
        CancellationToken cancellationToken;

        GameSettings gameSettings;

        List<ChatMessage> messagesToSend = new List<ChatMessage>();
        Func<object, bool> sendData;

        Level level;

        const int SendRate = 60;
        private int serverTick = 0;

        public Server(CancellationToken cancellationToken, GameSettings gameSettings, Func<object, bool> sendData = null)
        {
            this.cancellationToken = cancellationToken;
            this.gameSettings = gameSettings;
            this.sendData = sendData;

            var ip = gameSettings.GameIPAddress;
            
            listener = new TcpListener(ip, gameSettings.GamePort);
            listener.Start();
            
            world = World.Create();
            physicsWorld = new PhysicsWorld();

            level = new LevelGenerator(world, physicsWorld);
            level.PopulateLevel();

            queryDescriptions = new Dictionary<QueryDescriptions, QueryDescription>()
            {
                { QueryDescriptions.ModelRotator, new QueryDescription().WithAll<Rotation, ModelRotator>() },
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Camera, ClientInput, BodyHandle>() },
                { QueryDescriptions.DynamicPhysicsBodies, new QueryDescription().WithAll<Camera, BodyHandle>() },
                { QueryDescriptions.StaticPhysicsBodies, new QueryDescription().WithAll<StaticHandle>() }
            };

            updateSystems.Add(new PlayerInputSystem(world, queryDescriptions, physicsWorld));
            updateSystems.Add(new ModelRotatorSystem(world, queryDescriptions));
            updateSystems.Add(new PhysicsSystem(world, queryDescriptions, physicsWorld));
        }

        private void AddNewClients()
        {
            if(addNewClientTask == null)
            {
                addNewClientTask = Task.Run(async() =>
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

                    addNewClientTask = null;
                }, cancellationToken);
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
                                var box = new Box(2, 6.56f, 2);
                                var boxInertia = box.ComputeInertia(180);

                                var entity = world.Create(
                                    new Player() { Id = (uint)clients.Count + 1,
                                        Name = playerSettings.Name,
                                        Color = playerSettings.Color
                                    },
                                    new Camera() { Position = new Vector3(20 + clients.Count * 2, 0, 20) },
                                    new ClientInput(),
                                    new RenderModel() { Model = "capsule" },
                                    physicsWorld.AddBody(BodyDescription.CreateDynamic(new System.Numerics.Vector3(20 + clients.Count * 2, 0, 20), boxInertia, physicsWorld.Simulation.Shapes.Add(box), 0f))
                                );
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
                var data = serializer.Serialize(SerializableWorld.SerializeWorld(world, false));
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
                    if (client.entityReference.Entity.Has<BodyHandle>())
                    {
                        physicsWorld.RemoveBody(client.entityReference.Entity.Get<BodyHandle>());
                    }
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

                if (sendData != null)
                {
                    sendData(messagesToSend);
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

            if (sendData != null)
            {
                sendData(PlayersInfo);
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
                    physicsWorld.Dispose();
                    world?.Dispose();

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
