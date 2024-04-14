using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Strategy;
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
        stockfish = new Stockfish.NET.Core.Stockfish(stockFishFilePath);
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

        var chessMove = StockfishStrategyHelper.CreateMoveFromUci(uciString, ChessBoard);

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

    public void Dispose() 
    {
        stockfish.Dispose();
    }
}
