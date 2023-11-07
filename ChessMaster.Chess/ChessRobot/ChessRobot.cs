using ChessMaster.ChessDriver.Board;
using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;

namespace ChessMaster.ChessDriver
{
    public class ChessRobot : RobotSpace
    {
        private float chessBoardWidth;
        private float chessBoardHeight;
        private string portName = "";
        private CaptureSpace captureSpace;

        public ChessRobot(string portName, float chessBoardWidth, float chessBoardHeight, Space.Space captureSpace)
        {
            this.chessBoardWidth = chessBoardWidth;
            this.chessBoardHeight = chessBoardHeight;
            this.portName = portName;
            this.captureSpace = new CaptureSpace(captureSpace);
        }

        public void Initialize()
        {
            var chessBoard = new Board.ChessBoard(chessBoardWidth, chessBoardHeight);

            Initialize(new Space.Space[] { chessBoard, captureSpace.Space }, new RobotDriver.Robotic.Robot(new SerialDriver(portName)));
        }

        public void MoveFigureTo(SpacePosition figurePosition, SpacePosition targetPosition)
        { 
            MoveEntityFromSourceToTarget(figurePosition, targetPosition);
        }

        public void CaptureFigure(SpacePosition figurePosition)
        {
            var capturePosition = captureSpace.GetNextFreeSpace();
            MoveEntityFromSourceToTarget(figurePosition, capturePosition, targetSpaceIndex: 1);
        }
    }
}
