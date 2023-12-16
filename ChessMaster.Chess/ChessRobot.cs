using ChessMaster.Chess;
using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;
using System.Numerics;

namespace ChessMaster.ChessDriver
{
    public class ChessRobot : RobotSpace
    {
        private ChessBoard chessBoard;

        private float configurationHeight;

        public ChessRobot(string portName)
        {
            this.portName = portName;
            this.robot = new Robot(new SerialDriver(portName));
            this.chessBoard = new ChessBoard();
            this.space = chessBoard.Space;
        }
        public ChessRobot(IRobot robot)
        {
            this.robot = robot;
            this.chessBoard = new ChessBoard();
        }
        public void InitializeChessBoard(Vector2 a1Center, Vector2 h8Center)
        {
            chessBoard.Initialize(a1Center, h8Center);
        }
        public void InitializeMock()
        {
            //board.Initialize(new Vector2(50, 50), new Vector2(750, 750));
            //Initialize(new Space.Space[] { board, captureSpace.Space },
            //    robot);
        }
        public void Configure(Vector2 a1Center, Vector2 h8Center)
        {
            //robot.Move(100, 100, 100);
            //board.Initialize(new Vector2(20,20), new Vector2(750,500));
            ////20, 200 - h1

            ////var origin = new Vector3(420, 600, 20);
            ////var length = 57;
            ////a kedze druhy roh je 20, 200 tak delime ich rozdiel siedmymi
            ////robot.OpenGrip();QAW
            ////robot.MoveZ(100);
            ////robot.Move(418 - 342 + 57*3, 598, 100);

            //robot.MoveZ(100);
            //robot.OpenGrip();
        }
        
        public void MoveFigureTo(SpacePosition figurePosition, SpacePosition targetPosition)
        {
            MoveEntityFromSourceToTarget(figurePosition, targetPosition);
        }

        public void CaptureFigure(SpacePosition sourcePosition, SpacePosition targetPosition)
        {
            var freeSpace = chessBoard.GetNextFreeSpace();
            MoveEntityFromSourceToTarget(targetPosition, freeSpace);
            MoveEntityFromSourceToTarget(sourcePosition, targetPosition);
        }

        public void PromotePawn(SpacePosition source, SpacePosition target, FigureType promotion)
        {
            
        }

        public void ExecuteCastling(Castling castling)
        {
            MoveEntityFromSourceToTarget(castling.KingSource, castling.KingTarget);
            MoveEntityFromSourceToTarget(castling.RookSource, castling.RookTarget);
        }

        public void ConfigurationPickPawn()
        {
            //robot.OpenGrip();
            
            //configurationHeight = robot.GetState().Result.Position.Z;

            //robot.MoveZ(HeightProvider.GetHeight(FigureType.Pawn));

            //robot.CloseGrip();
        }

        public void ConfigurationReleasePawn()
        {
            //robot.OpenGrip();

            //robot.CloseGrip();

            //robot.MoveZ(configurationHeight);
        }
    }
}