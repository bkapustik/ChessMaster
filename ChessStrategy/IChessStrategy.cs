using ChessMaster.Space.Coordinations;

namespace ChessMaster.Chess;

public interface IChessStrategy
{
    Task Initialize();
    Task<Move> GetNextMove();
}
