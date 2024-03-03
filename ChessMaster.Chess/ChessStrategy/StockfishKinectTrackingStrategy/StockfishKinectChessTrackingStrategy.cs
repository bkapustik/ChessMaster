using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.ChessDriver.Strategy;

namespace ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;

public class StockfishKinectChessTrackingStrategy : IChessStrategy
{
    public MoveComputedEvent? MoveComputed { get; set; }
    
    public ChessMove Initialize()
    {
        return new StartGameMove();
    }
    public ChessMove InitializeFromOldGame(ChessBoardGeneral chessBoard)
    {
        throw new NotImplementedException();
    }
    public ChessBoardGeneral GetCurrentChessBoard()
    {
        throw new NotImplementedException();
    }
    public bool CanAcceptOldContext { get; }
 
    public void ComputeNextMove()
    {

    }
}
