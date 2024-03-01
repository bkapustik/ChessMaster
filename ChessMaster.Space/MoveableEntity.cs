using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.Space
{
    public abstract class MoveableEntity : Entity
    {
        public virtual Vector2 Get2DCenter()
        { 
            return Center2!.Value;
        }

        public virtual Vector3 GetHoldingPointVector()
        {
            return Center2!.Value.ToVector3();
        }
    }
}
