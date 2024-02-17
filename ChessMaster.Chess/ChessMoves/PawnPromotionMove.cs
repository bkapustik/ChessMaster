using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class PawnPromotionMove : ChessMove
{
    private readonly SpacePosition source;
    private readonly SpacePosition target;
    private readonly FigureType figureType;

    public PawnPromotionMove(SpacePosition source, SpacePosition target, FigureType figureType, bool isEndOfGame, string? message = null)
        : base(isEndOfGame, message)
    {
        this.source = source;
        this.target = target;
        this.figureType = figureType;
    }

    public override void Execute(ChessRobot robot)
    {
        robot.PromotePawn(source, target, figureType);
    }
}