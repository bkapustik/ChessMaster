using ChessMaster.Chess;
using ChessMaster.Space;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessFigure : MoveableEntity
{
    private const float RookHeight = 50;
    private const float RookPickupHeight = 25;
    private const float KnightHeight = 60;
    private const float KnightPickupHeight = 30;
    private const float BishopHeight = 44;
    private const float BishopPickupHeight = 22;
    private const float KingHeight = 100;
    private const float KingPickupHeight = 50;
    private const float QueenHeight = 50;
    private const float QueenPickupHeight = 25;
    private const float PawnHeight = 36;
    private const float PawnPickupHeight = 18;


    public ChessFigure(float tileWidth, float pickupHeight, float height)
    {
        Height = height;
        SetCenter(new Vector3(tileWidth / 2, tileWidth / 2, pickupHeight));
    }

    public static ChessFigure New(FigureType type, float tileWidth)
    { 
        switch (type) 
        {
            case FigureType.Rook: return new ChessFigure(tileWidth, RookPickupHeight, RookHeight);
            case FigureType.Bishop: return new ChessFigure(tileWidth, BishopPickupHeight, BishopHeight);
            case FigureType.Pawn: return new ChessFigure(tileWidth, PawnPickupHeight, PawnHeight);
            case FigureType.King: return new ChessFigure(tileWidth, KingPickupHeight, KingHeight);
            case FigureType.Queen: return new ChessFigure(tileWidth, QueenPickupHeight, QueenHeight);
            case FigureType.Knight: return new ChessFigure(tileWidth, KnightPickupHeight, KnightHeight);
            default: return new ChessFigure(tileWidth, PawnPickupHeight, PawnHeight);
        }
    }
}
