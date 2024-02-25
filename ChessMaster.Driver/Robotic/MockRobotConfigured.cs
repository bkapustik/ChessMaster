using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobotConfigured : MockRobot
{
    private float originX = 0, originY = 0, originZ = 0;
    public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }

    public MockRobotConfigured(Vector2 h8Position)
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
