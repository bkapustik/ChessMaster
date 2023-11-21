using System.Numerics;
using ChessMaster.Chess;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using ChessMaster.Space;

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

        public async Task Run()
        {
            var move = await chessStrategy.GetNextMove();
            bool isMoveDone = true;
            
            while (!move.IsEndOfGame)
            {
                if (isMoveDone)
                {
                    robot.MoveFigureTo(move.Source, move.Target);
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

        public void SwapStrategy(IChessStrategy chessStrategy)
        { 
            throw new NotImplementedException();
        }
    }
}
