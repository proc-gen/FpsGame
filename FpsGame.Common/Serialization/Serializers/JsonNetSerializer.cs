using Arch.Core;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FpsGame.Common.Serialization.ComponentConverters;

namespace FpsGame.Common.Serialization.Serializers
{
    public class JsonNetSerializer : ArchSerializer<string>
    {
        public JsonNetSerializer()
        {

        }

        public override void Deserialize(string data, SerializableWorld serializableWorld)
        {
            if (data.Length > 0)
            {
                SerializableWorld newSerializableWorld = null;
                List<SerializableEntity> newEntities = new List<SerializableEntity>();

                using (var sr = new StringReader(data))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        newSerializableWorld = serializer.Deserialize<SerializableWorld>(reader);
                    }
                }
                
                serializableWorld.PlayerId = newSerializableWorld.PlayerId;
                serializableWorld.MessageType = newSerializableWorld.MessageType;

                foreach (var entity in newSerializableWorld.Entities)
                {
                    var origEntity = serializableWorld.Entities.Where(a => a.SourceId == entity.SourceId).FirstOrDefault();
                    if (origEntity != null)
                    {
                        if(entity.SourceVersionId != origEntity.SourceVersionId)
                        {
                            origEntity.Delete = true;
                            newEntities.Add(entity);
                        }
                        else
                        {
                            origEntity.Update = true;
                            origEntity.Components = entity.Components;
                        }
                    }
                    else
                    {
                        entity.Create = true;
                        newEntities.Add(entity);
                    }
                }

                serializableWorld.Entities.AddRange(newEntities);
            }
        }

        public override string Serialize(SerializableWorld data)
        {
            string retVal;
            using (var sw = new StringWriter())
            {
                if (data != null && data.Entities.Any())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, data);
                    }

                }
                retVal = sw.ToString();
            }
            return retVal;
        }
    }
}
