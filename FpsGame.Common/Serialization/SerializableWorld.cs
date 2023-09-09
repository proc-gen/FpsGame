using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using FpsGame.Common.ClientData;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Serialization
{
    public class SerializableWorld : IMessageType
    {
        public List<SerializableEntity> Entities { get; set; }
        public List<SerializableEntity> EntitiesToRemove { get; set; }
        public uint PlayerId { get; set; }
        public string Type { get; set; }
        public bool FullLoad { get; set; }

        public SerializableWorld(bool full) 
        {
            Type = GetType().Name;
            FullLoad = full;
            Entities = new List<SerializableEntity>();
            EntitiesToRemove = new List<SerializableEntity>();
        }

        private readonly static QueryDescription allEntitiesQuery = new QueryDescription();
        private readonly static QueryDescription nonFullEntitiesQuery = new QueryDescription().WithNone<FullSerializeOnly>();

        public static SerializableWorld SerializeWorld(World world, bool full)
        {
            SerializableWorld serializableWorld = new SerializableWorld(full);

            world.Query(full ? allEntitiesQuery : nonFullEntitiesQuery, (in Entity entity) =>
            {
                var components = entity.GetAllComponents();
                if (components.Any(component => component is ISerializableComponent
                        && (full || (component is Remove) || ((ISerializableComponent)component).ComponentState != SerializableObjectState.NoChange)))
                {
                    serializableWorld.Entities.Add(SerializableEntity.SerializeEntity(entity, components, full));
                }
            });

            if (!full)
            {
                world.Query(in nonFullEntitiesQuery, (in Entity entity) =>
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

                world.Query(in nonFullEntitiesQuery, (in Entity entity) =>
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

        public string MessageType()
        {
            return Type;
        }

        
    }
}
