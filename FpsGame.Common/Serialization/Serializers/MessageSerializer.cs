using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization.Serializers
{
    public class MessageSerializer
    {
        NetworkStream stream;
        public MessageSerializer(NetworkStream stream) 
        {
            this.stream = stream;
        }

        public void Send(string data)
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
}
