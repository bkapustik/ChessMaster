using ChessMaster.RobotDriver.Robotic;

namespace ChessMaster.ChessDriver.Models;

public class MockRobotConfiguredDTO : RobotDTO
{
    public override IRobot GetRobot(string portName) => new MockRobotConfigured();
}
