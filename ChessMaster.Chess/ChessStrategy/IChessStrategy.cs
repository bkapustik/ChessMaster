using ChessMaster.ChessDriver.ChessMoves;

namespace ChessMaster.ChessDriver.Strategy;

public interface IChessStrategy
{
    ChessMove Initialize();
    ChessMove GetNextMove();
}
