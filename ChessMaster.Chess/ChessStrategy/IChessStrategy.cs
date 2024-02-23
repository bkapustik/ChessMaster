using ChessMaster.ChessDriver.ChessMoves;

namespace ChessMaster.ChessDriver.Strategy;

public interface IChessStrategy
{
    MoveComputedEvent? MoveComputed { get; set; }

    ChessMove Initialize();
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