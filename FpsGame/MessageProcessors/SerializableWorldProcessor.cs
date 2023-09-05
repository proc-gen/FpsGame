using Arch.Core;
using FpsGame.Common.Components;
using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using FpsGame.Common.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using FpsGame.Common.Serialization.Serializers;
using Arch.Core.Extensions;
using FpsGame.Common.Ecs;

namespace FpsGame.MessageProcessors
{
    public class SerializableWorldProcessor : IMessageProcessor
    {
        World world;
        SerializableWorld serializableWorld = new SerializableWorld(false);
        JsonNetArchSerializer serializer = new JsonNetArchSerializer();
        Dictionary<Type, Converter> converters;
        Dictionary<QueryDescriptions, QueryDescription> queryDescriptions;

        public EntityReference Player { get; private set; } = EntityReference.Null;

        public SerializableWorldProcessor(World world, Dictionary<QueryDescriptions, QueryDescription> queryDescriptions) 
        {
            this.world = world;
            this.queryDescriptions = queryDescriptions;

            converters = new Dictionary<Type, Converter>()
            {
                {typeof(RenderModel), new RenderModelConverter()},
                {typeof(Position), new PositionConverter()},
                {typeof(Rotation), new RotationConverter()},
                {typeof(Scale), new ScaleConverter()},
                {typeof(ModelRotator), new ModelRotatorConverter()},
                {typeof(Player), new PlayerConverter() },
                {typeof(Camera), new CameraConverter() },
            };
        }

        public void ProcessMessage(JObject data)
        {
            serializer.Deserialize(data, serializableWorld);

            if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Add).Any())
            {
                foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Add))
                {
                    Entity created = world.CreateFromArray(entity.GetDeserializedComponents(converters));
                    entity.EntityReference = created.Reference();
                    entity.DestinationId = created.Id;
                    entity.DestinationVersionId = created.Version();
                    entity.EntityState = SerializableObjectState.NoChange;
                }
            }

            if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Update).Any())
            {
                foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Update))
                {
                    world.SetFromArray(entity.EntityReference.Entity, entity.GetDeserializedComponents(converters));
                    entity.EntityState = SerializableObjectState.NoChange;
                }
            }

            if (serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Remove).Any())
            {
                foreach (var entity in serializableWorld.Entities.Where(a => a.EntityState == SerializableObjectState.Remove))
                {
                    world.Destroy(entity.EntityReference);
                    entity.EntityReference = EntityReference.Null;
                }

                serializableWorld.Entities.RemoveAll(a => a.EntityState == SerializableObjectState.Remove);
            }

            if (serializableWorld.EntitiesToRemove.Any())
            {
                foreach (var entity in serializableWorld.EntitiesToRemove)
                {
                    serializableWorld.Entities.RemoveAll(a => a.EntityReference == entity.EntityReference);
                    world.Destroy(entity.EntityReference);
                    entity.EntityReference = EntityReference.Null;
                }

                serializableWorld.EntitiesToRemove.Clear();
            }

            if (serializableWorld.FullLoad)
            {
                var playerQuery = queryDescriptions[QueryDescriptions.PlayerInput];

                world.Query(in playerQuery, (in Entity entity, ref Player player) =>
                {
                    if (player.Id == serializableWorld.PlayerId)
                    {
                        Player = entity.Reference();
                    }
                });
            }
        }
    }
}
