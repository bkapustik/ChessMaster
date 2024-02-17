using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Driver
{
    public class CommandsCompletedEventArgs : EventArgs
    {
        public bool Success;
        public RobotState RobotState;
    }
}
