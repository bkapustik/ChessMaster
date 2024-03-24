using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Events;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Services;
using ChessMaster.ChessDriver.Strategy;
using Stockfish.NET;

namespace ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;

public class StockfishKinectChessTrackingStrategy : IChessStrategy
{
    private readonly IKinectService kinectService;
    private readonly IStockfish stockfish;
    private bool IsRobotMove = false;
    private bool RobotMoveCompleted = true;

    private List<string> UciMoves = new List<string>();
    private string? LastExpectedMove = null;

    private ChessBoardGeneral ChessBoard = new ChessBoardGeneral();
    public MoveComputedEvent? MoveComputed { get; set; }

    public StockfishKinectChessTrackingStrategy(IKinectService kinectService, string stockFishFilePath)
    {
        this.kinectService = kinectService;
        stockfish = new Stockfish.NET.Stockfish(stockFishFilePath);

        kinectService.OnKinectMoveDetected += UpdateRecordStateFromKinectInput;
    }

    public void UpdateRecordStateFromKinectInput(object o, KinectMoveDetectedEventArgs e)
    {
        if (!IsRobotMove)
        {
            var newMoveUci = e.UciMoveString;

            UciMoves.Add(newMoveUci);

            var chessMove = StockfishStrategyHelper.CreateMoveFromUci(newMoveUci, ChessBoard);
            chessMove.StateUpdateOnly = true;

            IsRobotMove = true;
            HandleMoveComputed(true, chessMove);
        }
        else
        {
            if (LastExpectedMove == e.UciMoveString)
            {
                UciMoves.Add(e.UciMoveString);
                IsRobotMove = false;
                RobotMoveCompleted = true;
            }
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
        if (IsRobotMove && RobotMoveCompleted)
        {
            stockfish.SetPosition(UciMoves.ToArray());

            var uciString = stockfish.GetBestMoveTime(1500);

            if (uciString == null)
            {
                HandleMoveComputed(false, new ChessMove(false, "Check Mate"));
                return;
            }

            RobotMoveCompleted = false;

            var chessMove = StockfishStrategyHelper.CreateMoveFromUci(uciString, ChessBoard);
            LastExpectedMove = uciString;

            HandleMoveComputed(true, chessMove);
        }
    }

    private void OnMoveComputed(StrategyEventArgs e)
    {
        MoveComputed?.Invoke(this, e);
    }

    private void HandleMoveComputed(bool success, ChessMove move)
    {
        OnMoveComputed(new StrategyEventArgs(success, move));
    }

    public void Dispose()
    {
        kinectService.OnKinectMoveDetected -= UpdateRecordStateFromKinectInput;
    }
}
