using ChessMaster.RobotDriver.Robotic;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class PositionSetupState
{
    public RobotResponse RobotState { get; set; } = RobotResponse.NotInitialized;
    public bool IsSpedUp { get; set; }
    public bool IsA1Locked { get; set; }
    public bool IsH8Locked { get; set; }
    public Vector2 A1Position { get; set; }
    public Vector2 H8Position { get; set; }
    public Vector3 DesiredPosition { get; set; }
}