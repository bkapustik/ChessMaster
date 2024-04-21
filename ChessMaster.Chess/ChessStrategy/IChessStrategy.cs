using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;

namespace ChessMaster.ChessDriver.Strategy;

public interface IChessStrategy : IDisposable
{
    MoveComputedEvent? MoveComputed { get; set; }
    void InitializeFromOldGame(ChessBoardGeneral chessBoard, List<string> uciMoves);
    ChessBoardGeneral GetCurrentChessBoard();
    List<string> GetAllExecutedUciMoves();
    void Initialize();
    bool CanAcceptOldContext { get; }
    void ComputeNextMove();
}

public delegate void MoveComputedEvent(object? o, StrategyEventArgs e);

public class StrategyEventArgs : EventArgs
{ 
    public bool Success { get; set; }
    public ChessMove Move { get; set; }

    public StrategyEventArgs(bool success, ChessMove move)
    { 
        Success = success;
        Move = move;
    }
}