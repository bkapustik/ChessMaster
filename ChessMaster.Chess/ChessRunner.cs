using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ChessDriver.Events;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public sealed class ChessRunner
{
    /// <summary>
    /// Ensures the class is a singleton.
    /// </summary>
    public static ChessRunner Instance { get { return Nested.instance; } }

    private ChessRobot? ChessRobot { get; set; }
    private IChessStrategy? chessStrategy;
    private SemaphoreSlim semaphore;
    private ChessMove currentMove = new StartGameMove("Waiting for moves");
    private GameState gameState = GameState.NotInProgress;
    
    private Vector2 LastA1 { get; set; }
    private Vector2 LastH8 { get; set; }

    public bool RobotIsInitialized { get; private set; } = false;
    public bool GameHadBeenStarted { get; private set; } = false;
    public bool ChessBoardInitialized { get; private set; } = false;
    public float GetConfigurationHeight()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }
        
        return ChessRobot!.GetOrigin().Z;
    }
    private bool isMoveDone = true;
    private bool isMoveComputed = true;
    private bool isPaused = true;
    private RobotStateEvents? robotStateEvents;
    
    public RobotStateEvents? RobotStateEvents 
    { 
        get 
        { 
            if (ChessRobot == null)
            {
                throw new NullReferenceException("ChessRobot is null");
            } 
            return robotStateEvents;
        }

        private set
        { 
            robotStateEvents = value;
        } 
    }
    public MessageLoggedEvent? OnMessageLogged { get; set; }
    public GameStateEvent? OnGameStateChanged { get; set; }

    private ChessRunner() => semaphore = new SemaphoreSlim(1);
    public void SelectPort(IRobot robot)
    {
        ChessRobot = new ChessRobot(robot);
        RobotStateEvents = ChessRobot!.Events;
    }
    public void Start()
    {
        if (!RobotIsInitialized)
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
            GameHadBeenStarted = true;
            ChangeGameState(GameState.InProgress);
            Run();
        }
    }

    public List<ChessStrategyFacade> GetStrategies()
    {
        return new()
        {
            new PgnStrategyFacade(),
            new MockPgnStrategyFacade(),
            new StockFishStrategyFacade()
        };
    }
    public void InitializeChessBoard(Vector2 a1, Vector2 h8)
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("Robot is not initialized");
        }

        if (ChessBoardInitialized)
        {
            throw new InvalidOperationException("ChessBoard is already initialized");
        }

        LastA1 = a1;
        LastH8 = h8;

        ChessRobot!.InitializeChessBoard(a1, h8);
    }
    public void ReconfigureChessBoard(Vector2 a1, Vector2 h8)
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("Robot is not initialized");
        }

        if (!ChessBoardInitialized)
        {
            throw new InvalidOperationException("ChessBoard is not initialized yet.");
        }

        if (!isPaused)

        LastA1 = a1;
        LastH8 = h8;

        ChessRobot!.ReconfigureChessBoard(a1, h8);
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
    /// <returns>False if game is not paused or had not started yet or if robot is not initialized or new strategy can not accept old context.</returns>
    public bool TryChangeStrategyWithContext(IChessStrategy strategy, bool continueWithOldContext)
    {
        if (!isPaused || !GameHadBeenStarted || !RobotIsInitialized)
        {
            return false;
        }
        if (chessStrategy != null)
        {
            if (continueWithOldContext)
            {
                if (strategy.CanAcceptOldContext)
                {
                    //TODO zmenit
                    SwapStrategy(strategy, true);
                    SwapStrategy(strategy, false);
                }
                return false;
            }
            SwapStrategy(strategy, false);
        }
        return false;
    }
    public void Pause()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        semaphore.Wait();
        isPaused = true;
        semaphore.Release();
        ChessRobot!.Pause();
    }
    public void Resume()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        semaphore.Wait();
        isPaused = false;
        semaphore.Release();
        ChessRobot!.Resume();
    }
    public void Home()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        ChessRobot!.Home();
    }
    /// <summary>
    /// Picks up a figure if game is paused.
    /// </summary>
    /// <returns> True if game is paused and robot's state is <see cref="RobotResponse.Initialized"/> or <see cref="RobotResponse.Ok"/>. Otherwise false</returns>
    public bool TryPickFigure(FigureType figure)
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        if (CanPickFigure())
        {
            ChessRobot!.ConfigurationPickUpFigure(figure);
            return true;
        }
        return false;
    }
    public bool CanPickFigure()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        var state = ChessRobot!.GetState();
        return isPaused && (state.RobotResponse == RobotResponse.Ok || state.RobotResponse == RobotResponse.Initialized);
    }
    /// <summary>
    /// Releases a figure if game is paused.
    /// </summary>
    /// <returns> True if game is paused and robot's state is <see cref="RobotResponse.Initialized"/> or <see cref="RobotResponse.Ok"/>. Otherwise false</returns>
    public bool TryReleaseFigure(FigureType figure)
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        var state = ChessRobot!.GetState();
        
        if (isPaused && (state.RobotResponse == RobotResponse.Ok || state.RobotResponse == RobotResponse.Initialized))
        {
            ChessRobot!.ConfigurationReleaseFigure(figure);

            return true;
        }

        return false;
    }
    public void FinishMove()
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        semaphore.Wait();
        bool isPaused = this.isPaused;
        semaphore.Release();

        if (isPaused) 
        {
            ChessRobot!.Resume();
        }
    }
    public void ConfigurationMove(Vector3 desiredPosition)
    {
        if (ChessRobot == null || !RobotIsInitialized)
        {
            throw new InvalidOperationException("You must initialize the robot first");
        }

        ChessRobot!.Move(desiredPosition);
    }
    public void Initialize()
    {
        if (ChessRobot == null)
        {
            throw new NullReferenceException("Robot is null");
        }

        if (!RobotIsInitialized)
        {
            ChessRobot!.Initialize();
            RobotIsInitialized = true;
        }
    }
    public RobotState GetRobotState()
    {
        if (!RobotIsInitialized)
        {
            throw new RobotException("Robot is not initialized");
        }

        return ChessRobot!.GetState();
    }
    public bool IsRobotAtDesiredPosition(Vector3 desiredPosition)
    {
        if (!RobotIsInitialized)
        {
            throw new InvalidOperationException("Robot is not initialized");
        }

        return ChessRobot!.IsAtDesired(desiredPosition);
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
            InitializeChessBoard(LastA1, LastH8);
            InitializeStrategy(chessStrategy);
            chessStrategy!.ComputeNextMove();
        }
        else
        {
            throw new NotImplementedException();
        }

        isPaused = false;
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
        if (ChessRobot == null)
        {
            throw new InvalidOperationException("You must select the port first.");
        }

        ChessRobot!.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        {
            isMoveDone = true;
        }));

        while (!currentMove.IsEndOfGame)
        {
            try
            {
                semaphore.Wait();
                if (isMoveDone && isMoveComputed && !isPaused)
                {
                    semaphore.Release();
                    isMoveDone = false;
                    isMoveComputed = false;

                    currentMove.Execute(ChessRobot);
                    LogMove(new LogEventArgs(currentMove.Message ?? ""));
                    chessStrategy!.ComputeNextMove();
                }
                else
                {
                    semaphore.Release();
                    Thread.Sleep(100);
                }
            }
            catch (Exception e) 
            {
                throw new InvalidOperationException("Invalid move");
            }
        }
        ChangeGameState(GameState.NotInProgress);
    }
    private class Nested
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Nested()
        {
        }

        internal static readonly ChessRunner instance = new ChessRunner();
    }
}
