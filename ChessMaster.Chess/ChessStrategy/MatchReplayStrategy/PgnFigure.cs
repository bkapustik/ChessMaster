using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.Strategy;

public abstract class PgnFigure : IChessFigure
{
    protected readonly PgnTile[,] ChessBoard;

    public ChessColor ChessColor { get; set; }
    public FigureType FigureType { get; set; }

    public PgnFigure(PgnTile[,] chessBoard) => ChessBoard = chessBoard;

    public abstract bool CanMoveTo(Movement move);

    public static PgnFigure CreateFigure(ChessColor color, FigureType figure, PgnTile[,] chessBoard)
    {
        switch (figure)
        {
            case FigureType.Knight:
                return new Knight(chessBoard, color);
            case FigureType.Bishop:
                return new Bishop(chessBoard, color);
            case FigureType.Rook:
                return new Rook(chessBoard, color);
            case FigureType.King:
                return new King(chessBoard, color);
            case FigureType.Queen:
                return new Queen(chessBoard, color);
            default:
                return new Pawn(chessBoard, color);
        }
    }
}

public class Pawn : PgnFigure
{
    public Pawn(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        this.ChessColor = color;
        this.FigureType = FigureType.Pawn;
    }

    public override bool CanMoveTo(Movement move)
    {
        SpacePosition direction;
        SpacePosition origin;
        if (ChessColor == ChessColor.White)
        {
            direction = new SpacePosition(1, 0);
            origin = new SpacePosition(1, 0);
        }
        else
        {
            direction = new SpacePosition(-1, 0);
            origin = new SpacePosition(-1, 0);
        }
        return CanMoveToInternal(move, direction, origin);
    }

    private bool CanMoveToInternal(Movement move, SpacePosition direction, SpacePosition origin = default)
    {
        if (move.Source.Y == move.Target.Y)
        {
            if (move.Target.X == (origin + direction * 2).X)
            {
                if (ChessBoard[move.Target.X - 1, move.Target.Y].Figure == null)
                {
                    return true;
                }
                return false;
            }

            if ((move.Source + direction).X == move.Target.X && ChessBoard[move.Target.X - 1, move.Target.Y].Figure == null)
            {
                return true;
            }
            return false;
        }

        var captureDirection = new SpacePosition(direction.Y, direction.X);

        if (move.Source + captureDirection == move.Target
            && ChessBoard[move.Target.X, move.Target.Y].Figure != default
            && ChessBoard[move.Target.X, move.Target.Y].Figure!.ChessColor != ChessColor
            && ChessBoard[move.Target.X, move.Target.Y].Figure!.FigureType != FigureType.King)
        {
            return true;
        }
        return false;
    }
}

public class Knight : PgnFigure
{
    private static List<SpacePosition> possibleMoves = new List<SpacePosition> {
        new SpacePosition(1,2),
        new SpacePosition(1,-2),
        new SpacePosition(-1,2),
        new SpacePosition(-1,-2),
        new SpacePosition(2,1),
        new SpacePosition(2,-1),
        new SpacePosition(-2,1),
        new SpacePosition(-2,-1)
    };

    public Knight(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        this.ChessColor = color;
        this.FigureType = FigureType.Knight;
    }

    public override bool CanMoveTo(Movement move)
    {
        if (ChessBoard[move.Target.X, move.Target.Y].Figure == null)
        {
            if (IsInRange(move))
            {
                return true;
            }
            return false;
        }
        if (ChessBoard[move.Target.X, move.Target.Y].Figure!.ChessColor != ChessColor)
        {
            if (IsInRange(move))
            {
                return true;
            }
            return false;
        }
        return false;
    }

    private bool IsInRange(Movement move)
    {
        foreach (var vector in possibleMoves)
        {
            if (move.Source + vector == move.Target)
            {
                return true;
            }
        }
        return false;
    }
}

public class Queen : PgnFigure
{
    public Queen(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        this.ChessColor = color;
        this.FigureType = FigureType.Queen;
    }

    public override bool CanMoveTo(Movement move)
    {
        return true;
    }
}

public class King : PgnFigure
{
    public King(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        ChessColor = color;
        this.FigureType = FigureType.King;
    }
    public override bool CanMoveTo(Movement move)
    {
        return true;
    }
}

public class Bishop : PgnFigure
{
    public Bishop(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        ChessColor = color;
        this.FigureType = FigureType.Bishop;
    }
    public override bool CanMoveTo(Movement move)
    {
        if (ChessBoard[move.Source.X, move.Source.Y].ChessColor == ChessBoard[move.Target.X, move.Target.Y].ChessColor)
        {
            return true;
        }
        return false;
    }
}

public class Rook : PgnFigure
{
    public Rook(PgnTile[,] chessBoard, ChessColor color) : base(chessBoard)
    {
        ChessColor = color;
        this.FigureType = FigureType.Rook;
    }
    public override bool CanMoveTo(Movement move)
    {
        if (move.Source.X == move.Target.X)
        {
            if (move.Source.Y > move.Target.Y)
            {
                for (int i = move.Source.Y; i > move.Target.Y; i--)
                {
                    if (ChessBoard[move.Source.X, i].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            { 
                for (int i = move.Source.Y; i < move.Target.Y; i++)
                {
                    if (ChessBoard[move.Source.X, i].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        if (move.Source.Y == move.Target.Y)
        {
            if (move.Source.X > move.Target.X)
            {
                for (int i = move.Source.X; i > move.Target.X; i--)
                {
                    if (ChessBoard[i, move.Source.Y].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (int i = move.Source.X; i < move.Target.X; i++)
                {
                    if (ChessBoard[i, move.Source.Y].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }
}
