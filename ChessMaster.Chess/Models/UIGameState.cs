using ChessMaster.ChessDriver.Events;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class UIGameState
{
    public bool LogInitialized { get; set; } = false;
    public RobotResponse RobotState { get; set; } = RobotResponse.NotInitialized;
    public MovementState MovementState { get; set; } = MovementState.Unknown;
    public GameState GameState { get; set; } = GameState.NotInProgress;
    public bool IsSpedUp { get; set; }
    public bool IsA1Locked { get; set; }
    public bool IsH8Locked { get; set; }
    public Vector2 A1Position { get; set; }
    public Vector2 H8Position { get; set; }
    public Vector3 DesiredPosition { get; set; }

    public string GetA1String() => VectorToString(A1Position);
    public string GetH8String() => VectorToString(H8Position);
    private string VectorToString(Vector2 vector) => $"{vector.X}, {vector.Y}";
}