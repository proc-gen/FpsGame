using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using FpsGame.Common.Components;
using FpsGame.Common.Physics.Character;
using FpsGame.Common.Physics.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Physics
{
    public class PhysicsWorld : IDisposable
    {
        private BufferPool Pool;
        private ThreadDispatcher Dispatcher;
        public CharacterControllers characters { get; private set; }
        public Simulation Simulation { get; private set; }
        private bool disposedValue;

        public PhysicsWorld()
        {
            Pool = new BufferPool();
            var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            Dispatcher = new ThreadDispatcher(targetThreadCount);
            characters = new CharacterControllers(Pool);
            
            Simulation = Simulation.Create(Pool, new CharacterNarrowphaseCallbacks(characters), new PoseIntegratorCallbacks(new Vector3(0, -32, 0)), new SolveDescription(8, 4));
        }

        public const float timeStep = 1 / 60f;
        public void Step()
        {
            Simulation.Timestep(timeStep, Dispatcher);
        }

        public BodyHandle AddBody(BodyDescription description)
        {
            return Simulation.Bodies.Add(description);
        }

        public void RemoveBody(BodyHandle handle) 
        {
            Simulation.Bodies.Remove(handle);
        }

        public StaticHandle AddStatic(StaticDescription description)
        {
            return Simulation.Statics.Add(description);
        }

        public void RemoveStatic(StaticHandle handle)
        {
            Simulation.Statics.Remove(handle);
        }

        public CharacterInput AddCharacter(Vector3 initialPosition)
        {
            return AddCharacter(
                initialPosition, 
                new Capsule(1, 6.56f), 
                0.1f, 
                18, 
                11811.6f, 
                328, 
                19.69f, 
                13.12f, 
                MathF.PI * 0.4f
            );
        }

        public CharacterInput AddCharacter(
            Vector3 initialPosition, 
            Capsule shape,
            float minimumSpeculativeMargin, 
            float mass, 
            float maximumHorizontalForce, 
            float maximumVerticalGlueForce,
            float jumpVelocity, 
            float speed, 
            float maximumSlope = MathF.PI * 0.25f
        )
        {
            var shapeIndex = characters.Simulation.Shapes.Add(shape);
            var bodyHandle = characters.Simulation.Bodies.Add(
                BodyDescription.CreateDynamic(
                    initialPosition, 
                    new BodyInertia { InverseMass = 1f / mass },
                    new CollidableDescription(
                        shapeIndex, 
                        minimumSpeculativeMargin, 
                        float.MaxValue, 
                        ContinuousDetection.Passive
                    ), 
                    shape.Radius * 0.02f
                )
            );

            ref var character = ref characters.AllocateCharacter(bodyHandle);
            character.LocalUp = new Vector3(0, 1, 0);
            character.CosMaximumSlope = MathF.Cos(maximumSlope);
            character.JumpVelocity = jumpVelocity;
            character.MaximumVerticalForce = maximumVerticalGlueForce;
            character.MaximumHorizontalForce = maximumHorizontalForce;
            character.MinimumSupportDepth = shape.Radius * -0.01f;
            character.MinimumSupportContinuationDepth = -minimumSpeculativeMargin;

            return new CharacterInput()
            {
                Body = bodyHandle,
                Speed = speed,
                Shape = shape,
            };
        }

        public void RemoveCharacter(CharacterInput character)
        {
            characters.Simulation.Shapes.Remove(new BodyReference(character.Body, characters.Simulation.Bodies).Collidable.Shape);
            characters.Simulation.Bodies.Remove(character.Body);
            characters.RemoveCharacterByBodyHandle(character.Body);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    characters.Dispose();
                    Simulation.Dispose();
                    Dispatcher.Dispose();
                    Pool.Clear();
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
