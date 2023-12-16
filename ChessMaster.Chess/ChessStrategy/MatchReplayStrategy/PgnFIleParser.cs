using ChessMaster.Chess;
using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.Strategy;

public static class ChessFileParser
{
    public static async Task<Queue<PgnMove>> GetMoves(string filePath)
    {
        var queue = new Queue<PgnMove>();

        using (var streamReader = new StreamReader(filePath))
        {
            var line = await streamReader.ReadLineAsync();

            bool isCurrentWhite = true;

            while (line != null)
            {
                if (line.StartsWith('['))
                {
                    line = await streamReader.ReadLineAsync();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    line = await streamReader.ReadLineAsync();
                    continue;
                }

                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in tokens)
                {
                    if (token[0] == ';')
                    {
                        break;
                    }

                    if (token[0] != '{' && token[token.Length - 1] != '.' && !IsEndOfGameScoreString(token))
                    {
                        var move = ParseMove(token);
                        move.Color = isCurrentWhite ? ChessColor.White : ChessColor.Black;
                        queue.Enqueue(move);
                        isCurrentWhite = !isCurrentWhite;
                    }
                }

                line = await streamReader.ReadLineAsync();
            }
        }

        return queue;
    }

    public static PgnMove ParseMove(string moveString)
    {
        var move = new PgnMove();

        move.MoveType = GetMoveType(moveString);

        if (move.MoveType != MoveType.KingCastling && move.MoveType != MoveType.QueenSideCastling)
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
        if (move.MoveType == MoveType.KingCastling || move.MoveType == MoveType.QueenSideCastling)
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
                    Y = GetColumnFromLetter(moveString[0]),
                    X = -1
                };
            }
            else
            {
                move.Source = new SpacePosition()
                {
                    X = GetRowFromRank(moveString[0]),
                    Y = -1
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
                return MoveType.KingCastling;
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

    private static bool IsEndOfGameScoreString(string text)
    {
        if (!text.Contains('-'))
        {
            return false;
        }
        if (char.IsDigit(text[0]))
        {
            return true;
        }
        return false;
    }
}
