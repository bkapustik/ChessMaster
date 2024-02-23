using ChessMaster.Space.Coordinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space
{
    public abstract class Entity
    {
        public float? Width;
        public float? Length;
        public float? Height;
        public Vector2? Center2 { get; set; }

        public Vector3? Center3 { get; set; }
    }
}
