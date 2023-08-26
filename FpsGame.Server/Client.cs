using FpsGame.Common.Serialization;
using FpsGame.Common.ClientData;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FpsGame.Common.Serialization.Serializers;
using Newtonsoft.Json.Linq;

namespace FpsGame.Server
{
    public class Client : IDisposable
    {
        private bool disposedValue;

        private readonly TcpClient client;
        Func<JObject, bool> AddDataToProcess;
        GameSettings gameSettings;
        PlayerSettings playerSettings;

        MessageSerializer messageSerializer;

        public Client(Func<JObject, bool> addDataToProcess, GameSettings gameSettings, PlayerSettings playerSettings)
        {
            client = new TcpClient();
            AddDataToProcess = addDataToProcess;
            this.gameSettings = gameSettings;
            this.playerSettings = playerSettings;
        }

        public async Task Join(CancellationToken cancellationToken)
        {
            await client.ConnectAsync(gameSettings.GameIPAddress.First(), gameSettings.GamePort);
            messageSerializer = new MessageSerializer(client.GetStream(), AddDataToProcess);

            SendInputData(playerSettings);
            BeginReceiving(cancellationToken);
        }

        public async void BeginReceiving(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                messageSerializer.Receive();
            }
        }

        public void SendInputData(IMessageType clientInput)
        {
            messageSerializer.Send(clientInput);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageSerializer?.Dispose();
                    client?.Dispose();
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
