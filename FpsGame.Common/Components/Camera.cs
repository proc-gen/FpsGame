using FpsGame.Common.Constants;
using FpsGame.Common.Serialization.ComponentConverters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Components
{
    public struct Camera : ISerializableComponent
    {
        public Camera()
            : this(Vector3.Zero, -Vector3.UnitZ, 16/9f)
        {
        }

        public Camera(Vector3 position, Vector3 front, float aspectRatio)
        {
            Front = front;
            AspectRatio = aspectRatio;
            Up = Vector3.UnitY;
            Position = position;
        }

        public SerializableObjectState ComponentState { get; set; }

        public Vector3 position { get; set; }
        [IgnoreDataMember] 
        public Vector3 Position { get { return position; } set { position = value; UpdateVectors(); } }
        public Vector3 Front { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Right { get; set; }
        public float AspectRatio { get; set; }
        public float Pitch { get; set; } = 0f;
        [IgnoreDataMember] 
        public float PitchAsDegrees { get { return MathHelper.ToDegrees(Pitch); } set { Pitch = MathHelper.ToRadians(MathHelper.Clamp(value, -89f, 89f)); UpdateVectors(); } }
        public float Yaw { get; set; } = -MathHelper.PiOver2;
        [IgnoreDataMember]
        public float YawAsDegrees { get { return MathHelper.ToDegrees(Yaw); } set { Yaw = MathHelper.ToRadians(value); UpdateVectors(); } }
        public float Fov { get; set; } = MathHelper.PiOver2;
        [IgnoreDataMember]
        public float FovAsDegrees { get { return MathHelper.ToDegrees(Fov); } set { Fov = MathHelper.ToRadians(MathHelper.Clamp(value, 1f, 60f)); } }

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(Position, Front + Position, Up);
        }

        public Matrix GetProjectionMatrix()
        {
            return Matrix.CreatePerspectiveFieldOfView(Fov, AspectRatio, .001f, 100f);
        }

        private void UpdateVectors()
        {
            ComponentState = SerializableObjectState.Update;
            Front = new Vector3(
                MathF.Cos(Pitch) * MathF.Cos(Yaw),
                MathF.Sin(Pitch),
                MathF.Cos(Pitch) * MathF.Sin(Yaw)
            );

            Front.Normalize();
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
