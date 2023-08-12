using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization
{
    public class SerializableEntity
    {
        public int SourceId { get; set; }
        public int SourceVersionId { get; set; }
        public int DestinationId { get; set; }
        public int DestinationVersionId { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }
        public Dictionary<Type, object> Components { get; set; }

        public SerializableEntity() { }
        public SerializableEntity(int id, int versionId)
        {
            SourceId = id;
            SourceVersionId = versionId;
            Components = new Dictionary<Type, object>();
        }
        public SerializableEntity(int id, int versionId, Dictionary<Type, object> components)
        {
            SourceId = id;
            SourceVersionId = versionId;
            Components = components;
        }

        public object[] GetDeserializedComponents(Dictionary<Type, Converter> converters)
        {
            List<object> components = new List<object>();

            foreach (var component in Components)
            {
                components.Add(converters[component.Key].Convert(component.Value));
            }

            return components.ToArray();
        }
    }
}
