using ChessMaster.Robot.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Robot.Robot
{
    public class RobotEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public RobotState RobotState { get; set; }

        public RobotEventArgs(bool success, RobotState robotState)
        { 
            Success = success;
            RobotState = robotState;
        }
    }
}
