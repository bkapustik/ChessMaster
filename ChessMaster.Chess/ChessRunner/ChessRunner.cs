using ChessMaster.Chess.Strategy;

namespace ChessMaster.ChessDriver.Robot
{
    public class ChessRunner
    {
        private readonly ChessRobot robot;
        private IChessStrategy chessStrategy;

        public ChessRunner(IChessStrategy chessStrategy)
        {
            this.robot = new ChessRobot();
            this.chessStrategy = chessStrategy;
        }

        public void Work()
        {

        }

        public void SwapStrategy(IChessStrategy chessStrategy)
        { 
            throw new NotImplementedException();
        }
    }
}
