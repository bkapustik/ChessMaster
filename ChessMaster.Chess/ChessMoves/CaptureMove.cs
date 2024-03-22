using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class CaptureMove : ChessMove
{
    public CaptureMove(bool isEndOfGame, string? message = null)
        : base(isEndOfGame, message)
    { }
    public CaptureMove(SpacePosition source, SpacePosition target, bool isEndOfGame, string? message = null)
        : base(source, target, isEndOfGame, message)
    { }

    public override void Execute(ChessRobot robot)
    {
        robot.CaptureFigure(source, target, StateUpdateOnly);
    }
}