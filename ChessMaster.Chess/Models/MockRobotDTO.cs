using ChessMaster.RobotDriver.Robotic;

namespace ChessMaster.ChessDriver.Models;

public class MockRobotDTO : RobotDTO
{
    public override IRobot GetRobot(string portName) => new MockRobot();
}
