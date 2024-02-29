using ChessMaster.RobotDriver.Robotic;
using System.Numerics;

namespace ChessMaster.ChessDriver.Models;

public class RobotDTO
{
    public virtual IRobot GetRobot(string portName) => new Robot(portName);
    public virtual UIGameState GetSetupState() => new UIGameState();
    public virtual ConfigurationDTO GetConfiguration() => new ConfigurationDTO();
}

public class ConfigurationDTO
{
    public bool A1Locked { get; set; }
    public bool H8Locked { get; set; }
    public Vector2 A1Position { get; set; }
    public Vector2 H8Position { get; set; }
    public Vector3 RobotDesiredPosition { get; set; }
}