using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.Space.Coordinations;
using Stockfish.NET;

namespace ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;

public class StockfishAgainstStockfishStrategy : IChessStrategy
{
    private readonly IStockfish stockfish;
    private List<ChessMove> Moves = new List<ChessMove>();
    private ChessBoardGeneral ChessBoard = new ChessBoardGeneral();

    public MoveComputedEvent? MoveComputed { get; set; }

    public StockfishAgainstStockfishStrategy(string stockFishFilePath)
    {
        stockfish = new Stockfish.NET.Stockfish(stockFishFilePath);
    }

    public ChessMove Initialize()
    {
        ChessBoard.Initialize();
        return new StartGameMove("Game started");
    }
    public ChessMove InitializeFromOldGame(ChessBoardGeneral chessBoard)
    {
        ChessBoard = chessBoard;
        return new StartGameMove("Game started");
    }
    public ChessBoardGeneral GetCurrentChessBoard() => ChessBoard;

    public bool CanAcceptOldContext { get; }

    public void ComputeNextMove()
    {
        stockfish.SetPosition(Moves.Select(x => x.ToUci()).ToArray());

        var uciString = stockfish.GetBestMoveTime(1500);

        if (uciString == null)
        {
            HandleMoveComputed(false, new ChessMove(false, "Check Mate"));
            return;
        }

        var chessMove = CreateMoveFromUci(uciString);

        Moves.Add(chessMove);

        HandleMoveComputed(true, chessMove);
    }
    private void OnMoveComputed(StrategyEventArgs e)
    {
        MoveComputed?.Invoke(this, e);
    }

    private void HandleMoveComputed(bool success, ChessMove move)
    {
        Task.Run(() => { OnMoveComputed(new StrategyEventArgs(success, move)); });
    }

    private ChessMove CreateMoveFromUci(string uci)
    {
        var source = new SpacePosition(GetRow(uci[1]) - 1, GetColumn(uci[0]));
        var target = new SpacePosition(GetRow(uci[3]) - 1, GetColumn(uci[2]));


        if (uci.Length == 5)
        {
            var promotionFigure = GetPawnPromotionFigure(uci[4]);

            ChessBoard.Grid[target.Row, target.Column] = ChessBoard.Grid[source.Row, source.Column];
            ChessBoard.Grid[target.Row, target.Column]!.FigureType = promotionFigure;
            ChessBoard.Grid[source.Row, source.Column] = null;

            return new PawnPromotionMove(source, target, promotionFigure, false, uci);
        }

        if (ChessBoard.Grid[target.Row, target.Column] != null)
        {
            ChessBoard.Grid[target.Row, target.Column] = ChessBoard.Grid[source.Row, source.Column];
            ChessBoard.Grid[source.Row, source.Column] = null;

            return new CaptureMove(source, target, false, uci);
        }

        if (ChessBoard.Grid[source.Row, source.Column]!.FigureType == FigureType.King
            && Math.Abs(source.Row - target.Row) == 2
            )
        {
            var castlingMove = GetCastlingMove(source, target, uci);

            ChessBoard.Grid[target.Row, target.Column] = ChessBoard.Grid[source.Row, source.Column];
            ChessBoard.Grid[source.Row, source.Column] = null;

            ChessBoard.Grid[castlingMove.RookTarget.Row, castlingMove.RookTarget.Column] = ChessBoard.Grid[castlingMove.RookSource.Row, castlingMove.RookSource.Column];
            ChessBoard.Grid[castlingMove.RookSource.Row, castlingMove.RookSource.Column] = null;

            return new CastlingMove(castlingMove, false, uci);
        }

        ChessBoard.Grid[target.Row, target.Column] = ChessBoard.Grid[source.Row, source.Column];
        ChessBoard.Grid[source.Row, source.Column] = null;

        return new ChessMove(source, target, false, uci);
    }

    private int GetColumn(char col) => col - 'a';
    private int GetRow(char row) => row - '0';
    private FigureType GetPawnPromotionFigure(char castlingFigure)
    {
        switch (castlingFigure)
        {
            case 'R':
            case 'r': return FigureType.Rook;
            case 'N':
            case 'n': return FigureType.Knight;
            case 'B':
            case 'b': return FigureType.Bishop;
            case 'Q':
            case 'q': return FigureType.Queen;
            default: throw new InvalidOperationException($"Can not create Uci castling figure type from '{castlingFigure}'");
        }
    }
    private Castling GetCastlingMove(SpacePosition source, SpacePosition target, string uci)
    {
        if (source.Column != 4)
        {
            throw new InvalidOperationException("Invalid castling move");
        }

        if (source.Row == 0 || source.Row == 7)
        {
            if (target.Column == source.Column + 2)
            {
                return new Castling
                {
                    KingSource = source,
                    KingTarget = target,
                    RookSource = new SpacePosition(source.Row, target.Column + 3),
                    RookTarget = new SpacePosition(source.Row, target.Column + 1)
                };
            }
            else
            {
                return new Castling
                {
                    KingSource = source,
                    KingTarget = target,
                    RookSource = new SpacePosition(source.Row, 0),
                    RookTarget = new SpacePosition(source.Row, source.Column - 1)
                };
            }
        }

        throw new InvalidOperationException("Invalid castling move");
    }
}
