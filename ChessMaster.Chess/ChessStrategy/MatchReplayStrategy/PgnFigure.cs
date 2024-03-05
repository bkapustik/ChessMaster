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
        SpacePosition rowDirection;
        SpacePosition rowOrigin;
        if (ChessColor == ChessColor.White)
        {
            rowDirection = new SpacePosition(1, 0);
            rowOrigin = new SpacePosition(1, 0);
        }
        else
        {
            rowDirection = new SpacePosition(-1, 0);
            rowOrigin = new SpacePosition(6, 0);
        }
        return CanMoveToInternal(move, rowDirection, rowOrigin);
    }

    private bool CanMoveToInternal(Movement move, SpacePosition rowDirection, SpacePosition origin = default)
    {
        if (move.Source.Column == move.Target.Column)
        {
            if (move.Target.Row == (origin + rowDirection * 2).Row)
            {
                if (ChessBoard[move.Target.Row - 1, move.Target.Column].Figure == null)
                {
                    return true;
                }
                return false;
            }

            if ((move.Source + rowDirection).Row == move.Target.Row && ChessBoard[move.Target.Row - 1, move.Target.Column].Figure == null)
            {
                return true;
            }
            return false;
        }

        if ((move.Source + rowDirection).Row == move.Target.Row && Math.Abs(move.Source.Column - move.Target.Column) == 1
            && ChessBoard[move.Target.Row, move.Target.Column].Figure != default
            && ChessBoard[move.Target.Row, move.Target.Column].Figure!.ChessColor != ChessColor
            && ChessBoard[move.Target.Row, move.Target.Column].Figure!.FigureType != FigureType.King)
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
        if (ChessBoard[move.Target.Row, move.Target.Column].Figure == null)
        {
            if (IsInRange(move))
            {
                return true;
            }
            return false;
        }
        if (ChessBoard[move.Target.Row, move.Target.Column].Figure!.ChessColor != ChessColor)
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
        if (ChessBoard[move.Source.Row, move.Source.Column].ChessColor == ChessBoard[move.Target.Row, move.Target.Column].ChessColor)
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
        if (move.Source.Row == move.Target.Row)
        {
            if (move.Source.Column > move.Target.Column)
            {
                for (int i = move.Source.Column; i > move.Target.Column; i--)
                {
                    if (ChessBoard[move.Source.Row, i].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            { 
                for (int i = move.Source.Column; i < move.Target.Column; i++)
                {
                    if (ChessBoard[move.Source.Row, i].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        if (move.Source.Column == move.Target.Column)
        {
            if (move.Source.Row > move.Target.Row)
            {
                for (int i = move.Source.Row; i > move.Target.Row; i--)
                {
                    if (ChessBoard[i, move.Source.Column].Figure != null)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (int i = move.Source.Row; i < move.Target.Row; i++)
                {
                    if (ChessBoard[i, move.Source.Column].Figure != null)
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
