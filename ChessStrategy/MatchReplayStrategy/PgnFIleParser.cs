using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.Chess.Strategy.MatchReplay;

public static class ChessFileParser
{
    public static Queue<PgnMove> GetMoves(string filePath)
    {
        var queue = new Queue<PgnMove>();



        return queue;
    }

    public static PgnMove ParseMove(string moveString)
    {
        var move = new PgnMove();

        move.MoveType = GetMoveType(moveString);

        if (move.MoveType != MoveType.KingSideCastling && move.MoveType != MoveType.QueenSideCastling)
        {
            move.Figure = GetFigureType(moveString);

            if (char.IsUpper(moveString[0]))
            {
                moveString = moveString.Substring(1);
            }
        }

        move.CheckingMove = GetCheckingMove(moveString);

        if (move.CheckingMove != CheckingMove.None)
        {
            moveString = moveString.Substring(0, moveString.Length - 1);
        }
        if (move.MoveType == MoveType.KingSideCastling || move.MoveType == MoveType.QueenSideCastling)
        {
            return move;
        }
        if (move.MoveType == MoveType.Capture)
        {
            var capturePosition = moveString.IndexOf('x');
            moveString = $"{moveString.Substring(0, capturePosition)}{moveString.Substring(capturePosition + 1)}";
        }
        if (move.MoveType == MoveType.PawnPromotion)
        {
            var indexOfPromotionMark = moveString.IndexOf('=');
            move.PawnPromotion = new PawnPromotion()
            {
                FigureType = GetFigureTypeInternal(moveString[indexOfPromotionMark + 1])
            };

            moveString = $"{moveString.Substring(0, indexOfPromotionMark)}{moveString.Substring(indexOfPromotionMark + 2)}";
        }

        move.Target = new Space.Coordinations.SpacePosition()
        {
            X = GetRowFromRank(moveString[moveString.Length - 1]),
            Y = GetColumnFromLetter(moveString[moveString.Length - 2])
        };

        if (moveString.Length == 3)
        {
            if (char.IsLetter(moveString[0]))
            {
                move.Source = new SpacePosition()
                {
                    Y = GetColumnFromLetter(moveString[0])
                };
            }
            else
            {
                move.Source = new SpacePosition()
                {
                    X = GetRowFromRank(moveString[0])
                };
            }
        }
        else if (moveString.Length == 4)
        {
            move.Source = new Space.Coordinations.SpacePosition()
            {
                X = GetRowFromRank(moveString[1]),
                Y = GetColumnFromLetter(moveString[0])
            };
        }

        return move;
    }

    private static FigureType GetFigureType(string move)
    {
        if (char.IsUpper(move[0]))
        {
            return GetFigureTypeInternal(move[0]);
        }
        return FigureType.Pawn;
    }

    private static MoveType GetMoveType(string move)
    {
        if (move.Contains('x'))
        {
            return MoveType.Capture;
        }
        else if (move.Contains('='))
        {
            return MoveType.PawnPromotion;
        }
        else if (move.StartsWith('O'))
        {
            if (move.Count(x => x == 'O') == 2)
            {
                return MoveType.KingSideCastling;
            }
            return MoveType.QueenSideCastling;
        }
        else
        {
            return MoveType.Default;
        }
    }

    private static CheckingMove GetCheckingMove(string move)
    {
        if (move.Contains('+'))
        {
            return CheckingMove.Check;
        }
        if (move.Contains('#'))
        {
            return CheckingMove.CheckMate;
        }
        return CheckingMove.None;
    }

    private static FigureType GetFigureTypeInternal(char figureMark)
    {
        switch (figureMark)
        {
            case 'K':
                return FigureType.King;
            case 'P':
                return FigureType.Pawn;
            case 'R':
                return FigureType.Rook;
            case 'B':
                return FigureType.Bishop;
            case 'N':
                return FigureType.Knight;
            default:
                return FigureType.Queen;
        };
    }

    private static int GetRowFromRank(char rank)
    {
        return int.Parse(rank.ToString()) - 1;
    }

    private static int GetColumnFromLetter(char letter)
    {
        const int asciiaBase = 97;
        return (int)letter - asciiaBase;
    }
}
