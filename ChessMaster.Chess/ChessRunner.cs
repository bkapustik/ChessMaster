using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver
{
    public class ChessRunner
    {
        public readonly ChessRobot robot;
        private IChessStrategy? chessStrategy;

        public const string DUMMY_ROBOT = "DUMMY";

        public bool IsInitialized = false;

        public RobotState RobotState { get; set; }

        public ChessRunner(string portName)
        {
            if (portName == DUMMY_ROBOT)
            {
                robot = new ChessRobot(new MockRobot());
            }
            else
            {
                robot = new ChessRobot(portName);
            }
        }

        public ChessRunner(IRobot robot)
        {
            this.robot = new ChessRobot(robot);
        }

        public async Task Run()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("You must initialize the robot first");
            }
            if (chessStrategy is null)
            {
                throw new InvalidOperationException("You must initialize the chess strategy first");
            }

            var move = await chessStrategy.GetNextMove();
            bool isMoveDone = true;
            bool isEndOfGame = false;

            while (isEndOfGame)
            {
                if (move.IsEndOfGame)
                {
                    isEndOfGame = true;
                }
                else
                {
                    move.Execute(robot);

                    if (isMoveDone)
                    {
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

                    RobotState = robot.GetState();
                }
            }
        }

        public void Initialize()
        {
            robot.Initialize();
            IsInitialized = true;
        }

        public void InitializeMock()
        {
            robot.InitializeMock();
            IsInitialized = true;
        }

        public async Task InitializeStrategy(IChessStrategy chessStrategy)
        {
            this.chessStrategy = chessStrategy;
            await this.chessStrategy.Initialize();
        }

        public void Configure(Vector2 a1Center, Vector2 h8Center)
        {
            robot.Configure(a1Center, h8Center);
        }

        public void SwapStrategy(IChessStrategy chessStrategy)
        {
            throw new NotImplementedException();
        }

        public List<ChessStrategyFacade> GetStrategies()
        {
            return new()
            {
                new PgnStrategyFacade()
            };
        }

        public async Task PickStrategy(IChessStrategy strategy)
        {
            await InitializeStrategy(strategy);
        }
    }
}
