using ChessMaster.Chess;

namespace ChessMaster.ChessDriver.ChessMoves;

public class CastlingMove : ChessMove
{
    private readonly Castling castling;

    public CastlingMove(Castling castling, bool isEndOfGame, string? message = null)
        : base(isEndOfGame, message) => this.castling = castling;

    public override void Execute(ChessRobot robot)
    {
        robot.ExecuteCastling(castling);
    }
}
