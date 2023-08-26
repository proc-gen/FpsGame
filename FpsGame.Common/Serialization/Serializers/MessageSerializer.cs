using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Common.ClientData;

namespace FpsGame.Common.Serialization.Serializers
{
    public class MessageSerializer : IDisposable
    {
        private bool disposedValue;

        NetworkStream stream;
        Func<JObject, bool> AddDataToProcess;
        private BinaryReader reader;
        public MessageSerializer(NetworkStream stream, Func<JObject, bool> addDataToProcess) 
        {
            this.stream = stream;
            reader = new BinaryReader(stream);
            AddDataToProcess = addDataToProcess;
        }

        public void Send(string data)
        {
            if (stream.CanWrite)
            {
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write(data);
                    }

                    stream.Write(ms.ToArray());
                }
            }
        }

        public void Send(IMessageType data)
        {
            Send(Serialize(data));
        }

        public void Receive() 
        {
            if (stream.CanRead && stream.DataAvailable)
            {
                string message = reader.ReadString();
                if (!string.IsNullOrEmpty(message))
                {
                    using (var sr = new StringReader(message))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            JsonSerializer serializer = new JsonSerializer();

                            AddDataToProcess(serializer.Deserialize<JObject>(reader));
                        }
                    }
                }
            }
        }

        public string Serialize(IMessageType data)
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
