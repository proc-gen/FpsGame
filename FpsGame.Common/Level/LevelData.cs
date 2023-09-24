using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Common.Level
{
    public enum PhysicsObjectType
    {
        None = 0,
        Box = 1,
        Sphere = 2,
        Capsule = 3,
    }

    public class LevelData
    {
        public string LevelName { get; set; }
        public List<LevelModel> Models { get; set; }
        public List<LevelObject> LevelObjects { get; set; }
    }

    public class LevelModel
    {
        public string ModelName { get; set; }
    }

    public class LevelObject
    {
        public bool IsDynamic { get; set; }
        public string ModelName { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public PhysicsObjectType PhysicsObjectType { get; set; }
        public Vector4 PhysicsObjectParams { get; set; }
        public float Mass { get; set; }
    }
}
