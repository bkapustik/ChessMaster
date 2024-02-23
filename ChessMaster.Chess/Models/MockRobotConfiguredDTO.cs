using ChessMaster.RobotDriver.Robotic;
using System;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class MockRobotConfiguredDTO : RobotDTO
{
    private readonly Vector2 a1Position = new Vector2(100, 100);
    private readonly Vector2 h8Position = new Vector2(900, 900);

    public override IRobot GetRobot(string portName) => new MockRobotConfigured(h8Position);
    public override PositionSetupState GetSetupState() => new PositionSetupState
    {
        RobotState = RobotResponse.Initialized,
        IsA1Locked = true,
        IsH8Locked = true,
        A1Position = a1Position,
        H8Position = h8Position
    };
}
