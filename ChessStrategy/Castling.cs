using ChessMaster.Space.Coordinations;

namespace ChessMaster.Chess;

public struct Castling
{
    public SpacePosition KingSource { get; set; }
    public SpacePosition RookSource { get; set; }
    public SpacePosition KingTarget { get; set; }
    public SpacePosition RookTarget { get; set; }
}