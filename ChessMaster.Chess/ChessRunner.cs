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

public sealed class ChessRunner : IChessRunner
{
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

    public ChessRunner()
    {
        semaphore = new SemaphoreSlim(1);
    }

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
            new StockFishStrategyFacade(),
            new StockFishKinectStrategyFacade()
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
    public bool TryPickStrategy(IChessStrategy strategy)
    {
        if (chessStrategy != null)
        {
            return false;
        }

        InitializeStrategy(strategy);
        isPaused = false;
        return true;
    }
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

    private void SubscribeToStrategyMoveComputed(object? o, StrategyEventArgs e)
    {
        currentMove = e.Move;
        isMoveComputed = true;
    }

    private void InitializeStrategy(IChessStrategy chessStrategy)
    {
        if (this.chessStrategy != null)
        {
            this.chessStrategy!.MoveComputed -= SubscribeToStrategyMoveComputed;
        }

        this.chessStrategy = chessStrategy;
        var initializationResult = this.chessStrategy.Initialize();

        if (initializationResult.IsEndOfGame)
        {
            LogMove(new LogEventArgs(initializationResult.Message ?? ""));
        }

        isMoveDone = true;

        chessStrategy!.MoveComputed += SubscribeToStrategyMoveComputed;

        currentMove = new StartGameMove("Waiting for moves");
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
}
