using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class ChessMove
{
    protected readonly SpacePosition source;
    protected readonly SpacePosition target;
    public bool IsEndOfGame { get; set; }
    public string? Message { get; set; }

    public ChessMove(bool isEndOfGame, string? message = null)
    {
        IsEndOfGame = isEndOfGame;
        Message = message;
    }

    public ChessMove(SpacePosition source, SpacePosition target, bool isEndOfGame, string? message = null)
    {
        this.source = source;
        this.target = target;
        IsEndOfGame = isEndOfGame;
        Message = message;
    }

    public virtual void Execute(ChessRobot robot)
    {
        robot.MoveFigureTo(source, target);
    }
}