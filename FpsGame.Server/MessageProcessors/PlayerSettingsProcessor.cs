﻿using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.ClientData;
using FpsGame.Common.Components;
using FpsGame.Common.Physics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.MessageProcessors
{
    public class PlayerSettingsProcessor : IServerMessageProcessor
    {
        World world;
        PhysicsWorld physicsWorld;
        List<ChatMessage> messagesToSend;

        uint numClients = 0;

        public PlayerSettingsProcessor(World world, PhysicsWorld physicsWorld, List<ChatMessage> messagesToSend) 
        {
            this.world = world;
            this.physicsWorld = physicsWorld;
            this.messagesToSend = messagesToSend;
        }

        public void ProcessMessage(ClientData.ClientData message)
        {
            var playerSettings = message.Data.ToObject<PlayerSettings>();
            if (message.EntityReference == EntityReference.Null)
            {
                processNewPlayer(message, playerSettings);
            }
            else
            {
                processExistingPlayer(message, playerSettings);
            }
        }

        private void processNewPlayer(ClientData.ClientData message, PlayerSettings playerSettings)
        {
            var position = new System.Numerics.Vector3(20 + numClients * 2, 0, 20);

            var entity = world.Create(
                new Player()
                {
                    Id = numClients + 1,
                    Name = playerSettings.Name,
                    Color = playerSettings.Color,
                    MouseSensitivity = playerSettings.MouseSensitivity,
                    ControllerSensitivity = playerSettings.ControllerSensitivity,
                },
                new Camera() { Position = position + Vector3.UnitY * 2.5f },
                new Position() { X = position.X, Y = position.Y, Z = position.Z },
                new ClientInput(),
                new RenderModel() { Model = "capsule" },
                physicsWorld.AddCharacter(position)
            );
            message.Client.SetEntityReference(entity.Reference());
            messagesToSend.Add(new ChatMessage()
            {
                SenderName = "Server",
                Message = string.Format("{0} has connected", playerSettings.Name),
                Time = DateTime.Now,
            });
            numClients++;
        }

        private void processExistingPlayer(ClientData.ClientData message, PlayerSettings playerSettings)
        {
            var player = message.EntityReference.Entity.Get<Player>();
            player.Name = playerSettings.Name;
            player.Color = playerSettings.Color;
            player.MouseSensitivity = playerSettings.MouseSensitivity;
            player.ControllerSensitivity = playerSettings.ControllerSensitivity;
            message.EntityReference.Entity.Set(player);
        }
    }
}
