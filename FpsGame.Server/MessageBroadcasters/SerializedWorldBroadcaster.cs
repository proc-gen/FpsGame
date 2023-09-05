using Arch.Core;
using FpsGame.Common.ClientData;
using FpsGame.Common.Components;
using FpsGame.Common.Physics.Character;
using FpsGame.Common.Physics;
using FpsGame.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using FpsGame.Common.Serialization.Serializers;
using Arch.Core.Extensions;

namespace FpsGame.Server.MessageBroadcasters
{
    public class SerializedWorldBroadcaster : IServerMessageBroadcaster
    {
        GameSettings gameSettings;

        List<ServerSideClient> clients;
        List<ServerSideClient> newClients;
        JsonNetArchSerializer serializer = new JsonNetArchSerializer();

        World world;
        PhysicsWorld physicsWorld;
        List<ChatMessage> messagesToSend;

        public SerializedWorldBroadcaster(
            GameSettings gameSettings,
            List<ServerSideClient> clients,
            List<ServerSideClient> newClients,
            World world,
            PhysicsWorld physicsWorld,
            List<ChatMessage> messagesToSend) 
        {
            this.gameSettings = gameSettings;
            this.clients = clients;
            this.newClients = newClients;
            this.world = world;
            this.physicsWorld = physicsWorld;
            this.messagesToSend = messagesToSend;
        }

        public void Broadcast(int serverTick)
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
                    if (client.entityReference.Entity.Has<CharacterInput>())
                    {
                        physicsWorld.RemoveCharacter(client.entityReference.Entity.Get<CharacterInput>());
                    }
                    world.Destroy(client.entityReference.Entity);
                    client.SetEntityReference(EntityReference.Null);
                    client.Status = ServerSideClientStatus.Removed;
                }
            }
        }
    }
}
