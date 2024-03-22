using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;

namespace ChessMaster.ChessDriver.Strategy;

public interface IChessStrategy : IDisposable
{
    MoveComputedEvent? MoveComputed { get; set; }
    ChessMove InitializeFromOldGame(ChessBoardGeneral chessBoard);
    ChessBoardGeneral GetCurrentChessBoard();
    ChessMove Initialize();
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