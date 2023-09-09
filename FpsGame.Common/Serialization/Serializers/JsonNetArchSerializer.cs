using Arch.Core;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Constants;
using Newtonsoft.Json.Linq;

namespace FpsGame.Common.Serialization.Serializers
{
    public class JsonNetArchSerializer : ArchSerializer<string>
    {
        public JsonNetArchSerializer()
        {

        }

        public void Deserialize(JObject data, SerializableWorld serializableWorld)
        {
            SerializableWorld newSerializableWorld = data.ToObject<SerializableWorld>();

            Deserialize(serializableWorld, newSerializableWorld);
        }

        public override void Deserialize(string data, SerializableWorld serializableWorld)
        {
            if (data.Length > 0)
            {
                SerializableWorld newSerializableWorld = null;

                using (var sr = new StringReader(data))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        newSerializableWorld = serializer.Deserialize<SerializableWorld>(reader);
                    }
                }

                Deserialize(serializableWorld, newSerializableWorld);
            }
        }

        private void Deserialize(SerializableWorld serializableWorld, SerializableWorld newSerializableWorld)
        {
            List<SerializableEntity> newEntities = new List<SerializableEntity>();

            serializableWorld.PlayerId = newSerializableWorld.PlayerId;
            serializableWorld.FullLoad = newSerializableWorld.FullLoad;

            foreach (var entity in newSerializableWorld.Entities)
            {
                var origEntity = serializableWorld.Entities.Where(a => a.FullOnly == entity.FullOnly && a.SourceId == entity.SourceId).FirstOrDefault();
                if (origEntity != null)
                {
                    if (entity.EntityState == SerializableObjectState.Remove)
                    {
                        origEntity.EntityState = SerializableObjectState.Remove;
                        serializableWorld.EntitiesToRemove.Add(origEntity);
                    }
                    if (entity.SourceVersionId != origEntity.SourceVersionId)
                    {
                        origEntity.EntityState = SerializableObjectState.Remove;
                        newEntities.Add(entity);
                    }
                    else
                    {
                        origEntity.EntityState = SerializableObjectState.Update;
                        origEntity.Components = entity.Components;
                    }
                }
                else
                {
                    entity.EntityState = SerializableObjectState.Add;
                    newEntities.Add(entity);
                }
            }

            serializableWorld.Entities.AddRange(newEntities);
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
