using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.Robotic.Events;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.ChessDriver;

public class ChessRunner
{
    public readonly ChessRobot robot;
    private IChessStrategy? chessStrategy;

    public bool IsInitialized = false;

    public MessageLoggedEvent? OnMessageLogged { get; set; }

    public ChessRunner(IRobot robot)
    {
        this.robot = new ChessRobot(robot);
    }

    public void Run()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }
        if (chessStrategy is null)
        {
            throw new InvalidOperationException("You must initialize the chess strategy first");
        }
        bool isMoveDone = true;
        bool isMoveComputed = true;
        ChessMove move = new StartGameMove();

        chessStrategy.MoveComputed += (object? o, StrategyEventArgs e) =>
        {
            move = e.Move;
            isMoveComputed = true;
        };
        robot.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        {
            isMoveDone = true;
        }));
        chessStrategy.ComputeNextMove();

        while (!move.IsEndOfGame)
        {
            if (isMoveDone && isMoveComputed)
            {
                isMoveDone = false;
                isMoveComputed = false;
                
                move.Execute(robot);
                LogMove(new LogEventArgs(move.Message ?? ""));
                chessStrategy.ComputeNextMove();
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    public void Initialize()
    {
        robot.Initialize();
        IsInitialized = true;
    }

    public void InitializeStrategy(IChessStrategy chessStrategy)
    {
        this.chessStrategy = chessStrategy;
        var initializationResult = this.chessStrategy.Initialize();

        if (initializationResult.IsEndOfGame)
        {
            LogMove(new LogEventArgs(initializationResult.Message ?? ""));
        }
    }

    public void SwapStrategy(IChessStrategy chessStrategy)
    {
        throw new NotImplementedException();
    }

    public List<ChessStrategyFacade> GetStrategies()
    {
        return new()
        {
            new PgnStrategyFacade(),
            new MockPgnStrategyFacade()
        };
    }

    public void PickStrategy(IChessStrategy strategy)
    {
        InitializeStrategy(strategy);
    }

    private void LogMove(LogEventArgs e)
    {
        OnMessageLogged?.Invoke(this, e);
    }
}
