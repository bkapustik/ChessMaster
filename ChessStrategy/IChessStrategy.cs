using ChessMaster.Space.Coordinations;

namespace ChessMaster.Chess.Strategy;

public interface IChessStrategy
{
    Task Initialize();
    Task<Move> GetNextMove();
}
