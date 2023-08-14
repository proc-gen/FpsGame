using Arch.Core;
using Arch.Core.Extensions;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization
{
    public class SerializableWorld
    {
        public SerializableWorld() 
        {
            Entities = new List<SerializableEntity>();
        }

        private readonly static QueryDescription allEntitiesQuery = new QueryDescription();

        public static SerializableWorld SerializeWorld(World world, bool full)
        {
            SerializableWorld serializableWorld = new SerializableWorld();

            world.Query(in allEntitiesQuery, (in Entity entity) =>
            {
                var components = entity.GetAllComponents();
                if (components.Any(component => component is ISerializableComponent
                        && (full || ((ISerializableComponent)component).IsChanged)))
                {
                    var ec = new SerializableEntity(entity.Id, entity.Version());
                    foreach (var component in components)
                    {
                        if (component is ISerializableComponent
                            && (full || ((ISerializableComponent)component).IsChanged))
                        {
                            ec.Components.Add(component.GetType(), component);
                        }
                    }

                    serializableWorld.Entities.Add(ec);
                }
            });

            if (!full)
            {
                world.Query(in allEntitiesQuery, (in Entity entity) =>
                {
                    var components = entity.GetAllComponents();
                    if (components.Any(component => component is ISerializableComponent
                            && ((ISerializableComponent)component).IsChanged))
                    {
                        foreach (var component in components)
                        {
                            if (component is ISerializableComponent
                                && ((ISerializableComponent)component).IsChanged)
                            {
                                ((ISerializableComponent)component).IsChanged = false;
                            }
                        }

                        entity.SetRange(components);
                    }
                });
            }

            return serializableWorld;
        }

        public List<SerializableEntity> Entities;
    }
}
