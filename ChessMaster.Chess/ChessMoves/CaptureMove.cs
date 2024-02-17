using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class CaptureMove : ChessMove
{
    private readonly SpacePosition source;
    private readonly SpacePosition target;

    public CaptureMove(SpacePosition source, SpacePosition target, bool isEndOfGame, string? message = null)
        : base(isEndOfGame, message)
    {
        this.source = source;
        this.target = target;
    }

    public override void Execute(ChessRobot robot)
    {
        robot.CaptureFigure(source, target);
    }
}