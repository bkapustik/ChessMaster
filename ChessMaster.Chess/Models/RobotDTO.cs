using ChessMaster.RobotDriver.Robotic;

namespace ChessMaster.ChessDriver.Models;

public class RobotDTO
{
    public virtual IRobot GetRobot(string portName) => new Robot(portName);
    public virtual UIGameState GetSetupState() => new UIGameState();
}
