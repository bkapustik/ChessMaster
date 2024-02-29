using ChessMaster.ChessDriver.Events;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class UIGameState
{
    public bool ChessBoardInitialized { get; set; } = false;
    public bool LogInitialized { get; set; } = false;
    public RobotResponse RobotState { get; set; } = RobotResponse.NotInitialized;
    public MovementState MovementState { get; set; } = MovementState.Unknown;
    public GameState GameState { get; set; } = GameState.NotInProgress;
}