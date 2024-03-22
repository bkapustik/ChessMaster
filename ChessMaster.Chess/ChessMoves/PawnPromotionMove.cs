using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessMoves;

public class PawnPromotionMove : ChessMove
{
    private readonly FigureType figureType;

    public PawnPromotionMove(SpacePosition source, SpacePosition target, FigureType figureType, bool isEndOfGame, string? message = null)
        : base(source, target, isEndOfGame, message)
    {
        this.figureType = figureType;
    }

    public override void Execute(ChessRobot robot)
    {
        robot.PromotePawn(source, target, figureType, StateUpdateOnly);
    }

    public override string ToUci()
    {
        var source = base.source.ToUci();
        var target = base.target.ToUci();
        var promotion = figureType.ToUci();

        return $"{source}{target}{promotion}";
    }
}