using FpsGame.Common.Containers;
using FpsGame.Common.Serialization;
using FpsGame.Server.ClientData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FpsGame.Server
{
    public class Client : IDisposable
    {
        private bool disposedValue;

        private readonly TcpClient client;
        private NetworkStream stream;
        private BinaryReader reader;
        Func<string, bool> AddDataToProcess;
        PlayerSettings playerSettings;

        public Client(Func<string, bool> addDataToProcess, PlayerSettings playerSettings)
        {
            client = new TcpClient();
            AddDataToProcess = addDataToProcess;
            this.playerSettings = playerSettings;
        }

        public async Task Join(CancellationToken cancellationToken)
        {
            await client.ConnectAsync(IPAddress.Loopback, 1234);
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            SendInputData(playerSettings);
            BeginReceiving(cancellationToken);
        }

        public async void BeginReceiving(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (stream.DataAvailable)
                {
                    AddDataToProcess(reader.ReadString());
                }
            }
        }

        public void SendInputData(object clientInput)
        {
            
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Serialize(clientInput));
                }

                stream.Write(ms.ToArray());
            }
        }

        public string Serialize(object data)
        {
            string retVal;
            using (var sw = new StringWriter())
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, data);
                }

                retVal = sw.ToString();
            }
            return retVal;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader?.Dispose();
                    stream?.Dispose();
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
