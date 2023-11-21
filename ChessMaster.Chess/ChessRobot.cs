using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;
using System;
using System.Numerics;

namespace ChessMaster.ChessDriver
{
    public class ChessRobot : RobotSpace
    {
        private string portName;
        private CaptureSpace captureSpace;
        private ChessBoard board;

        private const int ChessTiles = 8;

        public ChessRobot(string portName, Space.Space captureSpace)
        {
            this.captureSpace = new CaptureSpace(captureSpace);
            this.portName = portName;
        }

        public ChessRobot(string portName, int chessBoardWidth, Space.Space captureSpace, SpacePosition origin,
            IHeightProvider heightProvider)
        {
            this.portName = portName;
            this.captureSpace = new CaptureSpace(captureSpace);
            board = new ChessBoard(ChessTiles, chessBoardWidth, origin, heightProvider);
        }

        public void Initialize()
        {
            Initialize(new Space.Space[] { board, captureSpace.Space },
                new RobotDriver.Robotic.Robot(new SerialDriver(portName)));
        }

        public void Configure()
        {
            //20, 200 - h1

            var origin = new Vector3(420, 600, 20);
            var length = 57;
            //a kedze druhy roh je 20, 200 tak delime ich rozdiel siedmymi
            //robot.OpenGrip();
            //robot.MoveZ(100);
            //robot.Move(418 - 342 + 57*3, 598, 100);

            robot.MoveZ(25);
            robot.OpenGrip();
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

        public void PromotePawn()
        {
        }
    }
}