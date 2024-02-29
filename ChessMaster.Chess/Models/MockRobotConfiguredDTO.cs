using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class MockRobotConfiguredDTO : RobotDTO
{
    private readonly Vector2 a1Position = new Vector2(100, 100);
    private readonly Vector2 h8Position = new Vector2(900, 900);

    public override IRobot GetRobot(string portName) => new MockRobotConfigured(h8Position);
    public override UIGameState GetSetupState() => new UIGameState
    {
        RobotState = RobotResponse.Initialized,
        MovementState = RobotDriver.State.MovementState.Idle,
        ChessBoardInitialized = true
    };

    public override ConfigurationDTO GetConfiguration() => new ConfigurationDTO
    {
        A1Locked = true,
        H8Locked = true,
        A1Position = a1Position,
        H8Position = h8Position,
        RobotDesiredPosition = h8Position.ToVector3()
    };
}
