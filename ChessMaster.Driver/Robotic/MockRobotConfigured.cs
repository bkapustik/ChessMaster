using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobotConfigured : MockRobot
{
    public MockRobotConfigured(Vector2 h8Position) : base()
    {
        displayedPosition = h8Position.ToVector3();
        SetupState = RobotResponse.Initialized;
    }

    public override void Initialize()
    {
        SetupState = RobotResponse.Initialized;
        MovementState = State.MovementState.Idle;
        HandleInitialized();
    }

    protected override void HandleInitialized()
    {
        Task.Run(() => HandleOkReponse());
    }
}
