using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space
{
    public abstract class MoveableEntity : Entity
    {
        public virtual Vector2 Get2DCenter()
        { 
            return Center!.Value;
        }

        public virtual Vector3 GetHoldingPointVector()
        {
            return Center3!.Value;
        }
    }
}
