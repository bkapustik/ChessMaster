using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver;
public class ChessMove
{
    private readonly SpacePosition source;
    private readonly SpacePosition target;
    public bool IsEndOfGame { get; set; }

    public ChessMove(bool isEndOfGame) 
    {
        IsEndOfGame = isEndOfGame;
    }

    public ChessMove(SpacePosition source, SpacePosition target, bool isEndOfGame)
    {
        this.source = source;
        this.target = target;
        this.IsEndOfGame = isEndOfGame;
    }

    public virtual void Execute(ChessRobot robot)
    {
        robot.MoveFigureTo(source, target);
    }
}

public class CastlingMove : ChessMove
{
    private readonly Castling castling;

    public CastlingMove(Castling castling, bool isEndOfGame) : base(isEndOfGame)
        => this.castling = castling;

    public override void Execute(ChessRobot robot)
    {
        robot.ExecuteCastling(castling);
    }
}

public class PawnPromotionMove : ChessMove
{
    private readonly SpacePosition source;
    private readonly SpacePosition target;
    private readonly FigureType figureType;

    public PawnPromotionMove(SpacePosition source, SpacePosition target, FigureType figureType, bool isEndOfGame) : base(isEndOfGame)
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

public class CaptureMove : ChessMove
{
    private readonly SpacePosition source;
    private readonly SpacePosition target;

    public CaptureMove(SpacePosition source, SpacePosition target, bool isEndOfGame) : base(isEndOfGame) 
    {
        this.source = source;
        this.target = target;
    }

    public override void Execute(ChessRobot robot)
    {
        robot.CaptureFigure(source, target);
    }
}