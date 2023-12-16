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

        private bool IsInitialized = false;

        private const string REPLAY_STRATEGY = "Replay Match";
        private const string HUMAN_VS_STOCKFISH_STRATEGY = "Human vs Robot";

        public RobotState RobotState { get; set; }

        public ChessRunner(string portName)
        {
            this.robot = new ChessRobot(portName);
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

                    RobotState = await robot.GetState();
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

        public async Task InitializeStrategy(IChessStrategy strategy)
        {
            this.chessStrategy = strategy;
            await chessStrategy.Initialize();
        }

        public void Configure(Vector2 a1Center, Vector2 h8Center)
        {
            robot.Configure(a1Center, h8Center);
        }

        public void SwapStrategy(IChessStrategy chessStrategy)
        {
            throw new NotImplementedException();
        }

        public List<string> GetStrategies()
        {
            return new() { 
                REPLAY_STRATEGY,
                HUMAN_VS_STOCKFISH_STRATEGY
            };
        }

        public async Task PickStrategy(string strategyName)
        {
            if (strategyName == REPLAY_STRATEGY)
            {
                await InitializeStrategy(new MatchReplayChessStrategy("C:\\Users\\asus\\Desktop\\Bakalarka\\ChessMaster\\Data\\Anatoly Karpov_vs_Garry Kasparov_1985.pgn"));
            }
            else if (strategyName == HUMAN_VS_STOCKFISH_STRATEGY)
            { 
                
            }
        }
    }
}
