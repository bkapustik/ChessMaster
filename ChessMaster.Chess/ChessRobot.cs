using ChessMaster.Chess;
using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;
using System.Net.Http.Headers;
using System.Numerics;

namespace ChessMaster.ChessDriver
{
    public class ChessRobot : RobotSpace
    {
        private string portName;

        private CaptureSpace captureSpace;
        private ChessBoard board;

        private const int boardIndex = 0;
        private const int captureSpaceIndex = 1;

        private const int ChessTiles = 8;

        public ChessRobot(string portName, Space.Space captureSpace)
        {
            this.captureSpace = new CaptureSpace(captureSpace);
            this.portName = portName;
            this.robot = new Robot(new SerialDriver(portName));
            this.board = new ChessBoard();
        }

        public ChessRobot(Space.Space captureSpace, IRobot robot)
        {
            this.captureSpace = new CaptureSpace(captureSpace);
            this.robot = robot;
            this.board = new ChessBoard();
        }

        public void Initialize()
        {
            Initialize(new Space.Space[] { board, captureSpace.Space },
                robot);
        }

        public void InitializeMock()
        {
            board.Initialize(new Vector2(50, 50), new Vector2(750, 750));
            Initialize(new Space.Space[] { board, captureSpace.Space },
                robot);
        }

        public void Configure(Vector2 a1Center, Vector2 h8Center)
        {
            robot.Move(100, 100, 100);
            board.Initialize(new Vector2(20,20), new Vector2(750,500));
            //20, 200 - h1

            //var origin = new Vector3(420, 600, 20);
            //var length = 57;
            //a kedze druhy roh je 20, 200 tak delime ich rozdiel siedmymi
            //robot.OpenGrip();
            //robot.MoveZ(100);
            //robot.Move(418 - 342 + 57*3, 598, 100);

            robot.MoveZ(100);
            robot.OpenGrip();
        }

        public void MoveFigureTo(SpacePosition figurePosition, SpacePosition targetPosition)
        {
            MoveEntityFromSourceToTarget(figurePosition, targetPosition);
        }

        public void CaptureFigure(SpacePosition sourcePosition, SpacePosition targetPosition)
        {
            var capturePosition = captureSpace.GetNextFreeSpace();
            MoveEntityFromSourceToTarget(targetPosition, capturePosition, targetSpaceIndex: captureSpaceIndex);
            MoveEntityFromSourceToTarget(sourcePosition, targetPosition);
        }

        public void PromotePawn(SpacePosition source, SpacePosition target, FigureType promotion)
        {
            var capturePosition = captureSpace.GetNextFreeSpace();
            MoveEntityFromSourceToTarget(source, capturePosition, targetSpaceIndex: captureSpaceIndex);
        }

        public void ExecuteCastling(Castling castling)
        {
            MoveEntityFromSourceToTarget(castling.KingSource, castling.KingTarget);
            MoveEntityFromSourceToTarget(castling.RookSource, castling.RookTarget);
        }
    }
}