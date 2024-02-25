using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Events;

public delegate void CommandsCompletedEvent(object? o, RobotEventArgs e);

public class CommandsCompletedEventArgs : EventArgs
{
    public bool Success;
    public RobotState RobotState;
}