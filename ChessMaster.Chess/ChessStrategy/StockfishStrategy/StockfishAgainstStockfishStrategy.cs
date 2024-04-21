using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Strategy;
using Stockfish.NET;

namespace ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;

public class StockfishAgainstStockfishStrategy : IChessStrategy
{
    private readonly IStockfish stockfish;
    private List<string> Moves = new List<string>();
    private ChessBoardGeneral ChessBoard = new ChessBoardGeneral();
    private StartGameMove? StartGameMove = null;

    public MoveComputedEvent? MoveComputed { get; set; }

    public StockfishAgainstStockfishStrategy(string stockFishFilePath)
    {
        stockfish = new Stockfish.NET.Core.Stockfish(stockFishFilePath);
    }

    public void Initialize()
    {
        ChessBoard.Initialize();
        StartGameMove = new StartGameMove("Game started");
    }
    public void InitializeFromOldGame(ChessBoardGeneral chessBoard, List<string> uciMoves)
    {
        Moves = uciMoves;
        ChessBoard = chessBoard;
        StartGameMove = new StartGameMove("Game initialized from previous game");
    }
    public ChessBoardGeneral GetCurrentChessBoard() => ChessBoard;

    public bool CanAcceptOldContext { get { return true; } }

    public List<string> GetAllExecutedUciMoves() => Moves;

    public void ComputeNextMove()
    {
        if (StartGameMove != null)
        {
            var startGameMove = StartGameMove;
            StartGameMove = null;
            HandleMoveComputed(true, startGameMove);
            return;
        }

        stockfish.SetPosition(Moves.ToArray());

        var uciString = stockfish.GetBestMoveTime(1500);

        if (uciString == null)
        {
            HandleMoveComputed(false, new ChessMove(false, "Check Mate"));
            return;
        }

        Moves.Add(uciString);

        var chessMove = StockfishStrategyHelper.CreateMoveFromUci(uciString, ChessBoard);

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
