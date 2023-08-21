using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization
{
    public class SerializableWorld : SerializableMessage
    {
        public SerializableWorld(bool full) 
        {
            MessageType = full ? Constants.MessageType.WorldFull : Constants.MessageType.WorldUpdate;
            Entities = new List<SerializableEntity>();
        }

        private readonly static QueryDescription allEntitiesQuery = new QueryDescription();

        public static SerializableWorld SerializeWorld(World world, bool full)
        {
            SerializableWorld serializableWorld = new SerializableWorld(full);


            world.Query(in allEntitiesQuery, (in Entity entity) =>
            {
                var components = entity.GetAllComponents();
                if (components.Any(component => component is ISerializableComponent
                        && (full || ((ISerializableComponent)component).ComponentState != SerializableObjectState.NoChange)))
                {
                    serializableWorld.Entities.Add(SerializableEntity.SerializeEntity(entity, components, full));
                }
            });

            if (!full)
            {
                world.Query(in allEntitiesQuery, (in Entity entity) =>
                {
                    var components = entity.GetAllComponents();
                    if (components.Any(component => component is ISerializableComponent
                            && ((ISerializableComponent)component).ComponentState == SerializableObjectState.Update))
                    {
                        foreach (var component in components)
                        {
                            if (component is ISerializableComponent
                                && ((ISerializableComponent)component).ComponentState == SerializableObjectState.Update)
                            {
                                ((ISerializableComponent)component).ComponentState = SerializableObjectState.NoChange;
                            }
                        }

                        entity.SetRange(components);
                    }
                });

                world.Query(in allEntitiesQuery, (in Entity entity) =>
                {
                    var components = entity.GetAllComponents();
                    if (components.Any(component => component is ISerializableComponent
                            && ((ISerializableComponent)component).ComponentState == SerializableObjectState.Remove))
                    {
                        List<ComponentType> componentsToRemove = new List<ComponentType>();

                        foreach (var component in components)
                        {
                            if (component is ISerializableComponent
                                && ((ISerializableComponent)component).ComponentState == SerializableObjectState.Remove)
                            {
                                ComponentType type;
                                if (!ComponentRegistry.TryGet(component.GetType(), out type))
                                {
                                    type = ComponentRegistry.Add(component.GetType());
                                }
                                componentsToRemove.Add(type);
                            }
                        }

                        world.RemoveRange(entity, componentsToRemove.ToArray());
                    }
                });
            }

            return serializableWorld;
        }

        public List<SerializableEntity> Entities;

        public uint PlayerId { get; set; }
    }
}
