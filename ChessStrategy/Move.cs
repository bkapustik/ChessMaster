using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.Chess;

public struct Move
{
    public SpacePosition Source { get; set; }
    public SpacePosition Target { get; set; }
    public bool IsEndOfGame { get; set; }
    public MoveType MoveType { get; set; }
    public PawnPromotion? PawnPromotion { get; set; }

    public Castling? Castling { get; set; }

    public Move(SpacePosition source, SpacePosition target)
    {
        Source = source;
        Target = target;
    }
}
