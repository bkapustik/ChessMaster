using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.Space
{
    public abstract class Entity
    {
        public float? Width;
        public float? Length;
        public float? Height;
        public Vector2? Center2 { get; private set; }

        public Vector3? Center3 { get; private set; }

        public void SetCenter(Vector2 center)
        { 
            Center2 = center;
            Center3 = center.ToVector3();
        }

        public void SetCenter(Vector3 center) 
        {
            Center3 = center;
            Center2 = center.ToVector2();
        }
    }
}
