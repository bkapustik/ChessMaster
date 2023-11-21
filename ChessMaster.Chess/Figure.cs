using ChessMaster.Space;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class Figure : MoveableEntity
{
    public Figure(float pickupHeight)
    { 
        Center3 = new Vector3(0, 0, pickupHeight);
    }
}
