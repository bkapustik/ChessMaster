using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space.Coordinations
{
    public struct SpacePosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        public SpacePosition()
        { 
        
        }

        public SpacePosition(int x, int y)
        { 
            X = x;
            Y = y;
        }

        public static SpacePosition operator +(SpacePosition a, SpacePosition b) => new SpacePosition(a.X + b.X, a.Y + b.Y);
        public static SpacePosition operator *(SpacePosition a, int b) => new SpacePosition(a.X * b, a.Y * b);
        public static bool operator ==(SpacePosition a, SpacePosition b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(SpacePosition a, SpacePosition b) => a.X != b.X || a.Y != b.Y;
    }
}
