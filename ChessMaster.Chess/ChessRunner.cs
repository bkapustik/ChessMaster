using ChessMaster.Chess;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver
{
    public class ChessRunner
    {
        private readonly ChessRobot robot;
        private IChessStrategy chessStrategy;

        private const int captureSpaceWidth = 0;
        private const int captureSpaceHeight = 0;

        public RobotState RobotState { get; set; }

        public ChessRunner(IChessStrategy chessStrategy, string portName)
        {
            var captureSpace = new Space.Space(captureSpaceWidth, captureSpaceHeight);

            this.robot = new ChessRobot(portName, captureSpace);
            this.chessStrategy = chessStrategy;
        }

        public ChessRunner(IChessStrategy chessStrategy, IRobot robot)
        {
            var captureSpace = new Space.Space(captureSpaceWidth, captureSpaceHeight);
            this.robot = new ChessRobot(captureSpace, robot);
            this.chessStrategy = chessStrategy;
        }

        public async Task Run()
        {
            var move = await chessStrategy.GetNextMove();
            bool isMoveDone = true;
            
            while (!move.IsEndOfGame)
            {
                if (isMoveDone)
                {
                    if (move.Castling != null)
                    {
                        robot.ExecuteCastling(move.Castling.Value);
                    }
                    else if (move.MoveType == Chess.Property.MoveType.PawnPromotion)
                    {
                        robot.PromotePawn(move.Source!.Value, move.Target!.Value, move.PawnPromotion!.Value.FigureType);
                    }
                    else if (move.MoveType == Chess.Property.MoveType.Capture)
                    {
                        robot.CaptureFigure(move.Source!.Value, move.Target!.Value);
                    }
                    else
                    {
                        robot.MoveFigureTo(move.Source!.Value, move.Target!.Value);
                    }

                    move = await chessStrategy.GetNextMove();
                    
                    isMoveDone = false;
                    
                    robot.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
                    {
                        isMoveDone = true;
                    }));
                }
                else
                {
                    await Task.Delay(100);
                }

                RobotState = await robot.GetState();
            }
        }

        public async Task Initialize()
        { 
            robot.Initialize();
            await chessStrategy.Initialize();
            RobotState = await robot.GetState();
        }

        public async Task InitializeMock()
        {
            robot.InitializeMock();
            await chessStrategy.Initialize();
            RobotState = await robot.GetState();
        }

        public void Configure(Vector2 a1Center, Vector2 h8Center)
        {
            robot.Configure(a1Center, h8Center);    
        }

        public void SwapStrategy(IChessStrategy chessStrategy)
        { 
            throw new NotImplementedException();
        }
    }
}
