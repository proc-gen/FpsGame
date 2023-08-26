﻿using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Components;
using FpsGame.Common.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FpsGame.Server
{
    public enum ServerSideClientStatus
    {
        Connected,
        JoinedGame,
        InGame,
        Disconnected,
        Removed,
    }

    public class ServerSideClient : IDisposable
    {
        private bool disposedValue;
        private TcpClient client;

        MessageSerializer messageSerializer;

        Func<ServerSideClient, JObject, bool> AddDataToProcess;
        public EntityReference entityReference { get; private set; }
        public ServerSideClientStatus Status { get; set; }

        public ServerSideClient(TcpClient client, Func<ServerSideClient, JObject, bool> addDataToProcess)
        {
            this.client = client;
            AddDataToProcess = addDataToProcess;
            messageSerializer = new MessageSerializer(client.GetStream(), addDataFromMessage);
            Status = ServerSideClientStatus.Connected;
        }

        private bool addDataFromMessage(JObject data)
        {
            return AddDataToProcess(this, data);
        }

        public void SetEntityReference(EntityReference entityReference)
        {
            if(Status == ServerSideClientStatus.Connected)
            {
                this.entityReference = entityReference;
                Status = ServerSideClientStatus.JoinedGame;
            }
        }

        public uint GetPlayerId()
        {
            return entityReference.Entity.Get<Player>().Id;
        }

        public void BeginReceiving(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    messageSerializer.Receive();
                }
                catch
                {
                    Disconnect();
                    return;
                }
            }
        }

        public void Disconnect()
        {
            Status = ServerSideClientStatus.Disconnected;
            client?.Close();
        }

        public void Send(string data)
        {
            messageSerializer.Send(data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageSerializer.Dispose();
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
