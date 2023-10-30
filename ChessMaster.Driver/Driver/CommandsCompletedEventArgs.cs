using ChessMaster.Robot.SerialResponse;
using ChessMaster.Robot.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Robot.Driver
{
    public class CommandsCompletedEventArgs : EventArgs
    {
        public bool Success;
        public RobotState RobotState;
    }
}
