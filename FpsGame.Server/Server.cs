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
using FpsGame.Common.Physics.Character;
using FpsGame.Server.MessageProcessors;
using FpsGame.Server.MessageBroadcasters;

namespace FpsGame.Server
{
    public class Server : IDisposable
    {
        bool disposedValue;
        List<ServerSideClient> clients = new List<ServerSideClient>();
        List<ServerSideClient> newClients = new List<ServerSideClient>();
        TcpListener listener;
      
        World world;
        PhysicsWorld physicsWorld;

        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;
        List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();
        Queue<ClientData.ClientData> ClientMessages = new Queue<ClientData.ClientData>();

        Dictionary<string, IServerMessageProcessor> MessageProcessors = new Dictionary<string, IServerMessageProcessor>();
        
        List<Task> tasks = new List<Task>();
        Task addNewClientTask = null;
        CancellationToken cancellationToken;

        GameSettings gameSettings;

        List<ChatMessage> messagesToSend = new List<ChatMessage>();
        Func<object, bool> sendData;

        Level level;

        int serverTick = 0;
        List<IServerMessageBroadcaster> broadcasters = new List<IServerMessageBroadcaster>();

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
                { QueryDescriptions.PlayerInput, new QueryDescription().WithAll<Player, Camera, ClientInput, CharacterInput>() },
                { QueryDescriptions.CharacterPhysicsBodies, new QueryDescription().WithAll<Camera, Position, CharacterInput>() },
                { QueryDescriptions.StaticPhysicsBodies, new QueryDescription().WithAll<StaticHandle>() },
                { QueryDescriptions.DynamicPhysicsBodies, new QueryDescription().WithAll<Position, Rotation, BodyHandle>() }
            };

            updateSystems.Add(new PlayerInputSystem(world, queryDescriptions, physicsWorld));
            updateSystems.Add(new ModelRotatorSystem(world, queryDescriptions));
            updateSystems.Add(new PhysicsSystem(world, queryDescriptions, physicsWorld));

            MessageProcessors.Add("PlayerSettings", new PlayerSettingsProcessor(world, physicsWorld, messagesToSend));
            MessageProcessors.Add("ClientInput", new ClientInputProcessor());
            MessageProcessors.Add("ClientDisconnect", new ClientDisconnectProcessor());

            broadcasters.Add(new SerializedWorldBroadcaster(gameSettings, clients, newClients, world, physicsWorld, messagesToSend));
            broadcasters.Add(new ChatMessageBroadcaster(clients, messagesToSend, sendData));
            broadcasters.Add(new PlayersInfoBroadcaster(clients, sendData));
        }

        public void Update(GameTime gameTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            serverTick++;

            ProcessClientMessages();

            foreach (var system in updateSystems)
            {
                system.Update(gameTime);
            }

            AddNewClients();

            foreach(var broadcaster in broadcasters)
            {
                broadcaster.Broadcast(serverTick);
            }
        }

        private void ProcessClientMessages()
        {
            if (ClientMessages.Count > 0)
            {
                do
                {
                    var message = ClientMessages.Dequeue();
                    if (message != null)
                    {
                        MessageProcessors[message.Data["Type"].ToString()].ProcessMessage(message);
                    }
                } while (ClientMessages.Count > 0);
            }
        }

        private void AddNewClients()
        {
            if (gameSettings.GameMode != GameMode.SinglePlayer || !clients.Any())
            {
                if (addNewClientTask == null)
                {
                    addNewClientTask = Task.Run(async () =>
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
