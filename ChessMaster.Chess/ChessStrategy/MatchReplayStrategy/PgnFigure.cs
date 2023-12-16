using ChessMaster.Chess;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.Strategy;

public abstract class PgnFigure : IChessFigure
{
    public ChessColor ChessColor { get; set; }
    public FigureType FigureType { get; set; }
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
                return new King(color);
            case FigureType.Queen:
                return new Queen(color);
            default:
                return new Pawn(chessBoard, color);
        }
    }
}

public class Pawn : PgnFigure
{
    private readonly PgnTile[,] chessBoard;

    public Pawn(PgnTile[,] chessBoard, ChessColor color)
    {
        this.chessBoard = chessBoard;
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
                if (chessBoard[move.Target.X - 1, move.Target.Y].Figure == null)
                {
                    return true;
                }
                return false;
            }

            if ((move.Source + direction).X == move.Target.X && chessBoard[move.Target.X - 1, move.Target.Y].Figure == null)
            {
                return true;
            }
            return false;
        }

        var captureDirection = new SpacePosition(direction.Y, direction.X);

        if (move.Source + captureDirection == move.Target
            && chessBoard[move.Target.X, move.Target.Y].Figure != default
            && chessBoard[move.Target.X, move.Target.Y].Figure!.ChessColor != ChessColor
            && chessBoard[move.Target.X, move.Target.Y].Figure!.FigureType != FigureType.King)
        {
            return true;
        }
        return false;
    }
}

public class Knight : PgnFigure
{
    private readonly PgnTile[,] chessBoard;

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

    public Knight(PgnTile[,] chessBoard, ChessColor color)
    {
        this.chessBoard = chessBoard;
        this.ChessColor = color;
        this.FigureType = FigureType.Knight;
    }

    public override bool CanMoveTo(Movement move)
    {
        if (chessBoard[move.Target.X, move.Target.Y].Figure == null)
        {
            if (IsInRange(move))
            {
                return true;
            }
            return false;
        }
        if (chessBoard[move.Target.X, move.Target.Y].Figure!.ChessColor != ChessColor)
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
    public Queen(ChessColor color)
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
    public King(ChessColor color)
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
    private readonly PgnTile[,] chessBoard;
    public Bishop(PgnTile[,] chessBoard, ChessColor color)
    {
        ChessColor = color;
        this.FigureType = FigureType.Bishop;
    }
    public override bool CanMoveTo(Movement move)
    {
        if (chessBoard[move.Source.X, move.Source.Y].ChessColor == chessBoard[move.Target.X, move.Target.Y].ChessColor)
        {
            return true;
        }
        return false;
    }
}

public class Rook : PgnFigure
{
    private readonly PgnTile[,] chessBoard;
    public Rook(PgnTile[,] chessBoard, ChessColor color)
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
                    if (chessBoard[move.Source.X, i].Figure != null)
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
                    if (chessBoard[move.Source.X, i].Figure != null)
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
                    if (chessBoard[i, move.Source.Y].Figure != null)
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
                    if (chessBoard[i, move.Source.Y].Figure != null)
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
