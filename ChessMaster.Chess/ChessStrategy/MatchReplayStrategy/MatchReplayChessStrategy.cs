﻿using ChessMaster.Chess;
using ChessMaster.Chess.Property;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.Strategy;

public class MatchReplayChessStrategy : IChessStrategy
{
    private readonly string filePath;
    private readonly PgnChessBoard chessBoard = new PgnChessBoard();

    private ChessParsingResult ChessParsingResult { get; set; }
    private ChessColor ColorOnMove = ChessColor.White;

    public MoveComputedEvent? MoveComputed { get; set; }

    public MatchReplayChessStrategy(string filePath)
    {
        this.filePath = filePath;
        this.chessBoard = new PgnChessBoard();
    }

    public bool CanAcceptOldContext
    {
        get { return false; }
    }

    public ChessMove Initialize()
    {
        ChessParsingResult = ChessFileParser.GetMoves(filePath);

        chessBoard.Initialize();

        if (ChessParsingResult.Moves.Count <= 0)
        {
            return new ChessMove(true, "Invalid Game");
        }

        return new ChessMove(false);
    }
    public ChessMove InitializeFromOldGame(ChessBoardGeneral chessBoard)
    {
        throw new NotImplementedException();
    }

    public void ComputeNextMove()
    {
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
                    Row = 0,
                    Column = 4
                };

                castling = new Castling()
                {
                    KingSource = whiteKingSource,
                    KingTarget = new SpacePosition(whiteKingSource.Row, whiteKingSource.Column + 2),
                    RookSource = new SpacePosition(whiteKingSource.Row, whiteKingSource.Column + 3),
                    RookTarget = new SpacePosition(whiteKingSource.Row, whiteKingSource.Column + 1)
                };
            }
            else
            {
                SpacePosition blackKingSource = new SpacePosition()
                {
                    Row = 7,
                    Column = 4
                };

                castling = new Castling()
                {
                    KingSource = blackKingSource,
                    KingTarget = new SpacePosition(blackKingSource.Row, blackKingSource.Column + 2),
                    RookSource = new SpacePosition(blackKingSource.Row, blackKingSource.Column + 3),
                    RookTarget = new SpacePosition(blackKingSource.Row, blackKingSource.Column + 1)
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
                    Row = 0,
                    Column = 4
                };

                castling = new Castling()
                {
                    KingSource = whiteKingSource,
                    KingTarget = new SpacePosition(whiteKingSource.Row, whiteKingSource.Column - 2),
                    RookSource = new SpacePosition(whiteKingSource.Row, 0),
                    RookTarget = new SpacePosition(whiteKingSource.Row, whiteKingSource.Column - 1)
                };
            }
            else
            {
                SpacePosition blackKingSource = new SpacePosition()
                {
                    Row = 7,
                    Column = 4
                };

                castling = new Castling()
                {
                    KingSource = blackKingSource,
                    KingTarget = new SpacePosition(blackKingSource.Row, blackKingSource.Column - 2),
                    RookSource = new SpacePosition(blackKingSource.Row, 0),
                    RookTarget = new SpacePosition(blackKingSource.Row, blackKingSource.Column - 1)
                };
            }

            FinishMove(castling);
            HandleMoveComputed(true, new CastlingMove(castling, false, pgnMove.Message));
            return;
        }

        FinishMove(source, target);
        HandleMoveComputed(true, new ChessMove(source, target, false, pgnMove.Message));
    }
    public ChessBoardGeneral GetCurrentChessBoard() => chessBoard.ToGeneral();

    public SpacePosition FindSourcePosition(PgnMove move)
    {
        if (move.Source is not null)
        {
            if (move.Source.Value.Row != -1 && move.Source.Value.Column != -1)
            {
                return move.Source.Value;
            }

            if (move.Source.Value.Row != -1)
            {
                for (int y = 0; y < PgnChessBoard.BoardDimLength; y++)
                {
                    var x = move.Source.Value.Row;
                    if (CanFigureMoveToTarget(x, y, move))
                    {
                        return new SpacePosition(x, y);
                    }
                }
            }
            if (move.Source.Value.Column != -1)
            {
                for (int x = 0; x < PgnChessBoard.BoardDimLength; x++)
                {
                    var y = move.Source.Value.Column;
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

        throw new InvalidOperationException("Wrong move");
    }

    private bool CanFigureMoveToTarget(int row, int column, PgnMove move)
    {
        if (chessBoard.Grid[row, column].Figure != null &&
                        chessBoard.Grid[row, column].Figure!.ChessColor == move.Color &&
                        chessBoard.Grid[row, column].Figure!.FigureType == move.Figure)
        {
            if (chessBoard.Grid[row, column].Figure!.CanMoveTo(new Movement(new SpacePosition(row, column), move.Target!.Value)))
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
        chessBoard.Grid[target.Row, target.Column].Figure = chessBoard.Grid[source.Row, source.Column].Figure;
        chessBoard.Grid[source.Row, source.Column].Figure = null;
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
        chessBoard.Grid[target.Row, target.Column].Figure!.FigureType = pawnPromotion;
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
