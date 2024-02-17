using ChessMaster.ChessDriver.Models;
using ChessMaster.RobotDriver.Robotic;
using System.Collections.Generic;

namespace ChessMaster.ControlApp.Helpers;

public class RobotPicker
{
    public const string MOCK_PORT = "MOCK";
    public const string MOCK_PORT_CONFIGURED = "MOCK_PORT_CONFIGURED";

    private Dictionary<string, RobotDTO> SelectedRobots { get; } = new Dictionary<string, RobotDTO>();
    public List<string> Ports { get; } = new List<string>();

    public RobotPicker()
    {
        AddPort(MOCK_PORT, new MockRobotDTO());
        AddPort(MOCK_PORT_CONFIGURED, new MockRobotConfiguredDTO());
    }

    public IRobot GetRobot(string portName) => SelectedRobots[portName].GetRobot(portName);

    public void AddPort(string portName)
    {
        Ports.Add(portName);
        SelectedRobots.Add(portName, new RobotDTO());
    }

    private void AddPort(string portName, RobotDTO robotDTO)
    {
        Ports.Add(portName);
        SelectedRobots.Add(portName, robotDTO);
    }
}