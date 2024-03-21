using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ChessDriver.Events;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public interface IChessRunner
{
    bool RobotIsInitialized { get; }
    bool GameHadBeenStarted { get; }
    bool ChessBoardInitialized { get; }
    RobotStateEvents? RobotStateEvents { get; }
    float GetConfigurationHeight();
    MessageLoggedEvent? OnMessageLogged { get; set; }
    GameStateEvent? OnGameStateChanged { get; set; }
    void SelectPort(IRobot robot);
    void Start();
    List<ChessStrategyFacade> GetStrategies();
    void InitializeChessBoard(Vector2 a1, Vector2 h8);
    void ReconfigureChessBoard(Vector2 a1, Vector2 h8);

    /// <summary>
    /// Picks and initializes a strategy
    /// </summary>
    /// <param name="strategy"></param>
    /// <returns>False if strategy has already been picked. Use <see cref="TryChangeStrategyWithContext"/>instead.</returns>
    bool TryPickStrategy(IChessStrategy strategy);

    /// <summary>
    /// Changes strategy to new strategy.
    /// </summary>
    /// <param name="strategy">New strategy</param>
    /// <param name="continueWithOldContext">Decides whether new strategy should continue with new context</param>
    /// <returns>False if game is not paused or had not started yet or if robot is not initialized or new strategy can not accept old context.</returns>
    bool TryChangeStrategyWithContext(IChessStrategy strategy, bool continueWithOldContext);
    /// <summary>
    /// Picks up a figure if game is paused.
    /// </summary>
    /// <returns> True if game is paused and robot's state is <see cref="RobotResponse.Initialized"/> or <see cref="RobotResponse.Ok"/>. Otherwise false</returns>
    public bool TryPickFigure(FigureType figure);
    void Pause();
    void Resume();
    void Home();
    bool CanPickFigure();
    /// <summary>
    /// Releases a figure if game is paused.
    /// </summary>
    /// <returns> True if game is paused and robot's state is <see cref="RobotResponse.Initialized"/> or <see cref="RobotResponse.Ok"/>. Otherwise false</returns>
    public bool TryReleaseFigure(FigureType figure);
    void FinishMove();
    void ConfigurationMove(Vector3 desiredPosition);
    void Initialize();
    RobotState GetRobotState();
    bool IsRobotAtDesiredPosition(Vector3 desiredPosition);
}
