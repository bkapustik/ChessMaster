using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Services;
using ChessMaster.ChessDriver.Strategy;
using ChessTracking.Core.Services.Events;
using Stockfish.NET;

namespace ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;

public class StockfishKinectChessTrackingStrategy : IChessStrategy
{
    private readonly IKinectService kinectService;
    private readonly IStockfish stockfish;
    private bool IsRobotMove = false;
    private List<string> UciMoves = new List<string>();

    private ChessBoardGeneral ChessBoard = new ChessBoardGeneral();
    public MoveComputedEvent? MoveComputed { get; set; }

    public StockfishKinectChessTrackingStrategy(IKinectService kinectService, string stockFishFilePath)
    {
        this.kinectService = kinectService;
        stockfish = new Stockfish.NET.Stockfish(stockFishFilePath);

        kinectService.GameController.TrackingProcessor.OnRecordStateUpdated += UpdateRecordStateFromKinectInput;
    }

    public void UpdateRecordStateFromKinectInput(object o, RecordStateUpdatedEventArgs e)
    { 
        if (!IsRobotMove)
        {
            var newMoveUci = e.RecordOfGame.Last();

            if (newMoveUci == UciMoves.Last())
            {
                return;
            }

            UciMoves.Add(newMoveUci);

            var chessMove = StockfishStrategyHelper.CreateMoveFromUci(newMoveUci, ChessBoard);
            chessMove.StateUpdateOnly = true;

            HandleMoveComputed(true, chessMove);

            IsRobotMove = true;
        }
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
        if (IsRobotMove)
        {
            stockfish.SetPosition(UciMoves.ToArray());

            var uciString = stockfish.GetBestMoveTime(1500);

            if (uciString == null)
            {
                HandleMoveComputed(false, new ChessMove(false, "Check Mate"));
                return;
            }

            UciMoves.Add(uciString);

            var chessMove = StockfishStrategyHelper.CreateMoveFromUci(uciString, ChessBoard);

            HandleMoveComputed(true, chessMove);
        }
    }

    private void OnMoveComputed(StrategyEventArgs e)
    {
        MoveComputed?.Invoke(this, e);
    }

    private void HandleMoveComputed(bool success, ChessMove move)
    {
        Task.Run(() => { OnMoveComputed(new StrategyEventArgs(success, move)); });

        IsRobotMove = false;
    }

    public void Dispose()
    {
        kinectService.GameController.TrackingProcessor.OnRecordStateUpdated -= UpdateRecordStateFromKinectInput;
    }
}
