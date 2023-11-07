using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
