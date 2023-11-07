using ChessMaster.RobotDriver.SerialResponse;
using ChessMaster.RobotDriver.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.RobotDriver.Driver
{
    public class CommandsCompletedEventArgs : EventArgs
    {
        public bool Success;
        public RobotState RobotState;
    }
}
