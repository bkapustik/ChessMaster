using ChessMaster.Space.Coordinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space
{
    public class SubSpace : Entity
    {
        public MoveableEntity? Entity { get; set; }
        public Vector3 GetCenter()
        {
            return new Vector3(Width, Length, Height);
        }
    }
}
