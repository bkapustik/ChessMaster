using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class CastlingMove : ChessMove
{
    private readonly Castling castling;
    public CastlingMove(Castling castling, bool isEndOfGame, string? message = null) : base(isEndOfGame, message)
    {
        this.castling = castling;
    }

    public CastlingMove(bool isEndOfGame, string? message = null)
       : base(isEndOfGame, message)
    { }
    public CastlingMove(SpacePosition source, SpacePosition target, bool isEndOfGame, string? message = null)
        : base(source, target, isEndOfGame, message)
    { }

    public override void Execute(ChessRobot robot)
    {
        robot.ExecuteCastling(castling);
    }
}
