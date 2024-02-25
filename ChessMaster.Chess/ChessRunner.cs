using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ChessDriver.Events;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.Robotic;

namespace ChessMaster.ChessDriver;

public class ChessRunner
{
    public readonly ChessRobot robot;
    private IChessStrategy? chessStrategy;
    private SemaphoreSlim semaphore;
    private ChessMove currentMove = new StartGameMove("Waiting for moves");
    private GameState gameState = GameState.NotInProgress;

    public bool IsInitialized { get; private set; } = false;
    public bool HadBeenStarted { get; private set; } = false;

    private bool isPaused = false;
    private bool isMoveDone = true;
    private bool isMoveComputed = true;

    public MessageLoggedEvent? OnMessageLogged { get; set; }
    public GameStateEvent? OnGameStateChanged { get; set; }

    public ChessRunner(IRobot robot)
    {
        this.robot = new ChessRobot(robot);
        semaphore = new SemaphoreSlim(1);
    }

    public void Start()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }
        if (chessStrategy is null)
        {
            throw new InvalidOperationException("You must initialize the chess strategy first");
        }
        if (gameState == GameState.InProgress)
        {
            Resume();
        }
        else
        {
            HadBeenStarted = true;
            ChangeGameState(GameState.InProgress);
            Run();
        }
    }

    public List<ChessStrategyFacade> GetStrategies()
    {
        return new()
        {
            new PgnStrategyFacade(),
            new MockPgnStrategyFacade()
        };
    }

    /// <summary>
    /// Picks and initializes a strategy
    /// </summary>
    /// <param name="strategy"></param>
    /// <returns>False if strategy has already been picked. Use <see cref="TryChangeStrategyWithContext"/>instead.</returns>
    public bool TryPickStrategy(IChessStrategy strategy)
    {
        if (chessStrategy != null)
        {
            return false;
        }

        InitializeStrategy(strategy);
        return true;
    }

    /// <summary>
    /// Changes strategy to new strategy.
    /// </summary>
    /// <param name="strategy">New strategy</param>
    /// <param name="continueWithOldContext">Decides whether new strategy should continue with new context</param>
    /// <returns>False if game is not paused or had note started yet or if robot is not initialized or new strategy can not accept old context.</returns>
    public bool TryChangeStrategyWithContext(IChessStrategy strategy, bool continueWithOldContext)
    {
        if (!isPaused || !HadBeenStarted || !IsInitialized)
        {
            return false;
        }
        if (chessStrategy != null)
        {
            if (continueWithOldContext)
            {
                if (strategy.CanAcceptOldContext)
                {
                    SwapStrategy(strategy, true);
                }
                return false;
            }
            SwapStrategy(strategy, false);
        }
        return false;
    }

    public void Pause()
    {
        semaphore.Wait();
        isPaused = true;
        semaphore.Release();
        robot?.Robot?.Pause();
    }

    public void Resume()
    {
        semaphore.Wait();
        isPaused = false;
        semaphore.Release();
        robot?.Robot?.Resume();
    }

    public void FinishMove()
    {
        semaphore.Wait();
        bool isPaused = this.isPaused;
        semaphore.Release();
        if (isPaused) 
        {
            robot?.Robot?.Resume();
        }
    }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            robot.Initialize();
            IsInitialized = true;
        }
    }

    private void InitializeStrategy(IChessStrategy chessStrategy)
    {
        this.chessStrategy = chessStrategy;
        var initializationResult = this.chessStrategy.Initialize();

        if (initializationResult.IsEndOfGame)
        {
            LogMove(new LogEventArgs(initializationResult.Message ?? ""));
        }

        isMoveDone = true;

        chessStrategy!.MoveComputed += (object? o, StrategyEventArgs e) =>
        {
            currentMove = e.Move;
            isMoveComputed = true;
        };

        isPaused = false;
    }

    private void SwapStrategy(IChessStrategy chessStrategy, bool continueWithOldContext)
    {
        if (!continueWithOldContext)
        {
            InitializeStrategy(chessStrategy);
            chessStrategy!.ComputeNextMove();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void LogMove(LogEventArgs e)
    {
        OnMessageLogged?.Invoke(this, e);
    }

    private void ChangeGameState(GameState newState)
    {
        gameState = newState;
        OnGameStateChanged?.Invoke(this, new GameStateEventArgs(newState));
    }

    private void Run()
    {
        robot.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        {
            isMoveDone = true;
        }));

        while (!currentMove.IsEndOfGame)
        {
            semaphore.Wait();
            if (isMoveDone && isMoveComputed && !isPaused)
            {
                semaphore.Release();
                isMoveDone = false;
                isMoveComputed = false;

                currentMove.Execute(robot);
                LogMove(new LogEventArgs(currentMove.Message ?? ""));
                chessStrategy!.ComputeNextMove();
            }
            else
            {
                semaphore.Release();
                Thread.Sleep(100);
            }
        }
        ChangeGameState(GameState.NotInProgress);
    }
}
