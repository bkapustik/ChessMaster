using ChessMaster.Chess.Property;
using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessStrategy.MatchReplayStrategy
{
    public class MockMatchReplayChessStrategy : IChessStrategy
    {
        private readonly byte[] fileData;
        private readonly PgnChessBoard chessBoard = new PgnChessBoard();
        private bool IsPlayerMove = false;

        private ChessParsingResult ChessParsingResult { get; set; }
        private ChessColor ColorOnMove = ChessColor.White;

        public MoveComputedEvent? MoveComputed { get; set; }

        public MockMatchReplayChessStrategy(byte[] fileData)
        {
            this.fileData = fileData;
            this.chessBoard = new PgnChessBoard();
        }

        public ChessMove Initialize()
        {
            ChessParsingResult = ChessFileParser.GetMoves(fileData);

            chessBoard.Initialize();

            if (ChessParsingResult.Moves.Count <= 0)
            {
                return new ChessMove(true, "Invalid Game");
            }

            return new ChessMove(false);
        }

        public void ComputeNextMove()
        {
            if (IsPlayerMove)
            {
                Thread.Sleep(2000);
                IsPlayerMove = false;
            }
            else 
            {
                IsPlayerMove = true;
            }

            if (ChessParsingResult.Moves.Count == 0)
            {
                HandleMoveComputed(true, new ChessMove(true, $"End of the game. Result: {ChessParsingResult.MatchResultMessage}"));
                return;
            }

            var pgnMove = ChessParsingResult.Moves.Dequeue();

            SpacePosition source = new SpacePosition();
            SpacePosition target = new SpacePosition();

            if (pgnMove.Source != null ||
                pgnMove.MoveType == MoveType.Default ||
                pgnMove.MoveType == MoveType.Capture ||
                pgnMove.MoveType == MoveType.PawnPromotion)
            {
                source = FindSourcePosition(pgnMove);
            }

            if (pgnMove.Target != null)
            {
                target = pgnMove.Target.Value;
            }

            if (pgnMove.MoveType == MoveType.PawnPromotion)
            {
                AdvanceColor();

                FinishMove(source, target, pgnMove.PawnPromotion!.Value.FigureType);
                HandleMoveComputed(true, new PawnPromotionMove(source, target, pgnMove.PawnPromotion!.Value.FigureType, false, pgnMove.Message));
                return;
            }

            if (pgnMove.MoveType == MoveType.Capture)
            {
                FinishMove(source, target);
                HandleMoveComputed(true, new CaptureMove(source, target, false, pgnMove.Message));
                return;
            }

            var castling = new Castling();

            if (pgnMove.MoveType == MoveType.KingCastling)
            {
                if (ColorOnMove == ChessColor.White)
                {
                    SpacePosition whiteKingSource = new SpacePosition()
                    {
                        X = 4,
                        Y = 0
                    };

                    castling = new Castling()
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

                    castling = new Castling()
                    {
                        KingSource = blackKingSource,
                        KingTarget = new SpacePosition(blackKingSource.X + 2, blackKingSource.Y),
                        RookSource = new SpacePosition(blackKingSource.X + 3, blackKingSource.Y),
                        RookTarget = new SpacePosition(blackKingSource.X + 1, blackKingSource.Y)
                    };
                }

                FinishMove(castling);
                HandleMoveComputed(true, new CastlingMove(castling, false, pgnMove.Message));
                return;
            }
            else if (pgnMove.MoveType == MoveType.QueenSideCastling)
            {
                if (ColorOnMove == ChessColor.White)
                {
                    SpacePosition whiteKingSource = new SpacePosition()
                    {
                        X = 4,
                        Y = 0
                    };

                    castling = new Castling()
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

                    castling = new Castling()
                    {
                        KingSource = blackKingSource,
                        KingTarget = new SpacePosition(blackKingSource.X - 2, blackKingSource.Y),
                        RookSource = new SpacePosition(0, blackKingSource.Y),
                        RookTarget = new SpacePosition(blackKingSource.X - 1, blackKingSource.Y)
                    };
                }

                FinishMove(castling);
                HandleMoveComputed(true, new CastlingMove(castling, false, pgnMove.Message));
                return;
            }

            FinishMove(source, target);
            HandleMoveComputed(true, new ChessMove(source, target, false, pgnMove.Message));
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
            if (ColorOnMove == ChessColor.White)
            {
                ColorOnMove = ChessColor.Black;
                return;
            }
            ColorOnMove = ChessColor.White;
        }

        private void AdvanceFigureState(SpacePosition source, SpacePosition target)
        {
            chessBoard.Grid[target.X, target.Y].Figure = chessBoard.Grid[source.X, source.Y].Figure;
            chessBoard.Grid[source.X, source.Y].Figure = null;
        }

        private void FinishMove(Castling castling)
        {
            AdvanceColor();

            AdvanceFigureState(castling.KingSource, castling.KingTarget);
            AdvanceFigureState(castling.RookSource, castling.RookTarget);
        }

        private void FinishMove(SpacePosition source, SpacePosition target)
        {
            AdvanceColor();

            AdvanceFigureState(source, target);
        }

        private void FinishMove(SpacePosition source, SpacePosition target, FigureType pawnPromotion)
        {
            FinishMove(source, target);
            chessBoard.Grid[target.X, target.Y].Figure!.FigureType = pawnPromotion;
        }

        private void OnMoveComputed(StrategyEventArgs e)
        {
            MoveComputed?.Invoke(this, e);
        }

        private void HandleMoveComputed(bool success, ChessMove move)
        {
            Task.Run(() => { OnMoveComputed(new StrategyEventArgs(success, move)); });
        }
    }
}
