using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;
using System.Runtime.CompilerServices;

namespace ChessMaster.Chess.Strategy.MatchReplay;

public class MatchReplayChessStrategy : IChessStrategy
{
    private Queue<PgnMove> moves;
    private readonly string filePath;
    private ChessColor colorOnMove = ChessColor.White;
    private readonly PgnChessBoard chessBoard;

    public MatchReplayChessStrategy(string filePath)
    {
        this.filePath = filePath;
        this.chessBoard = new PgnChessBoard();
    }

    public async Task Initialize()
    {
        moves = await ChessFileParser.GetMoves(filePath);
        chessBoard.Initialize();
    }

    public async Task<Move> GetNextMove()
    {
        var move = new Move();

        if (moves.Count <= 0)
        {
            move.IsEndOfGame = true;
            return move;
        }

        var pgnMove = moves.Dequeue();

        move.IsEndOfGame = pgnMove.IsEnfOfGame;
        move.PawnPromotion = pgnMove.PawnPromotion;
        move.MoveType = pgnMove.MoveType;

        if (pgnMove.Source != null ||
            pgnMove.MoveType == MoveType.Default ||
            pgnMove.MoveType == MoveType.Capture ||
            pgnMove.MoveType == MoveType.PawnPromotion)
        {
            move.Source = FindSourcePosition(pgnMove);
        }

        if (pgnMove.Target != null)
        {
            move.Target = pgnMove.Target.Value;
        }

        if (pgnMove.MoveType == MoveType.KingCastling)
        {
            if (colorOnMove == ChessColor.White)
            {
                SpacePosition whiteKingSource = new SpacePosition()
                {
                    X = 4,
                    Y = 0
                };

                move.Castling = new Castling()
                {
                    KingSource = whiteKingSource,
                    KingTarget = new SpacePosition(whiteKingSource.X + 2, whiteKingSource.Y),
                    RookSource = new SpacePosition(whiteKingSource.X + 3, whiteKingSource.Y),
                    RookTarget = new SpacePosition(whiteKingSource.X + 1, whiteKingSource.Y)
                };
            }
            else
            {
                SpacePosition blackKingSource = new SpacePosition()
                {
                    X = 4,
                    Y = 7
                };

                move.Castling = new Castling()
                {
                    KingSource = blackKingSource,
                    KingTarget = new SpacePosition(blackKingSource.X + 2, blackKingSource.Y),
                    RookSource = new SpacePosition(blackKingSource.X + 3, blackKingSource.Y),
                    RookTarget = new SpacePosition(blackKingSource.X + 1, blackKingSource.Y)
                };
            }
        }
        else if (pgnMove.MoveType == MoveType.QueenSideCastling)
        {
            if (colorOnMove == ChessColor.White)
            {
                SpacePosition whiteKingSource = new SpacePosition()
                {
                    X = 4,
                    Y = 0
                };

                move.Castling = new Castling()
                {
                    KingSource = whiteKingSource,
                    KingTarget = new SpacePosition(whiteKingSource.X - 2, whiteKingSource.Y),
                    RookSource = new SpacePosition(0, whiteKingSource.Y),
                    RookTarget = new SpacePosition(whiteKingSource.X - 1, whiteKingSource.Y)
                };
            }
            else
            {
                SpacePosition blackKingSource = new SpacePosition()
                {
                    X = 4,
                    Y = 7
                };

                move.Castling = new Castling()
                {
                    KingSource = blackKingSource,
                    KingTarget = new SpacePosition(blackKingSource.X - 2, blackKingSource.Y),
                    RookSource = new SpacePosition(0, blackKingSource.Y),
                    RookTarget = new SpacePosition(blackKingSource.X - 1, blackKingSource.Y)
                };
            }
        }

        AdvanceColor();

        return move;
    }
    public SpacePosition FindSourcePosition(PgnMove move)
    {
        if (move.Source is not null)
        {
            if (move.Source.Value.X != -1 && move.Source.Value.Y != -1)
            {
                return move.Source.Value;
            }

            if (move.Source.Value.X != -1)
            {
                for (int y = 0; y < PgnChessBoard.BoardDimLength; y++)
                {
                    var x = move.Source.Value.X;
                    if (CanFigureMoveToTarget(x, y, move))
                    {
                        return new SpacePosition(x, y);
                    }
                }

                for (int x = 0; x < PgnChessBoard.BoardDimLength; x++)
                {
                    var y = move.Source.Value.Y;
                    if (CanFigureMoveToTarget(x, y, move))
                    {
                        return new SpacePosition(x, y);
                    }
                }
            }
        }
        for (int x = 0; x < PgnChessBoard.BoardDimLength; x++)
        {
            for (int y = 0; y < PgnChessBoard.BoardDimLength; y++)
            {
                if (CanFigureMoveToTarget(x, y, move))
                {
                    return new SpacePosition(x, y);
                }
            }
        }

        return default;
    }

    private bool CanFigureMoveToTarget(int x, int y, PgnMove move)
    {
        if (chessBoard.Grid[x, y].Figure != null &&
                        chessBoard.Grid[x, y].Figure!.ChessColor == move.Color &&
                        chessBoard.Grid[x, y].Figure!.FigureType == move.Figure)
        {
            if (chessBoard.Grid[x, y].Figure!.CanMoveTo(new Movement(new SpacePosition(x, y), move.Target!.Value)))
            {
                return true;
            }
        }
        return false;
    }

    private void AdvanceColor()
    {
        if (colorOnMove == ChessColor.White)
        {
            colorOnMove = ChessColor.Black;
            return;
        }
        colorOnMove = ChessColor.White;
    }
}
