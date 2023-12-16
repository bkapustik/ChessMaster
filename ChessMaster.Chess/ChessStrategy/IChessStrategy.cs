namespace ChessMaster.ChessDriver.Strategy;

public interface IChessStrategy
{
    Task Initialize();
    Task<ChessMove> GetNextMove();
}
