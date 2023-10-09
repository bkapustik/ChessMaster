using ChessMaster.Robot.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Robot.SerialResponse
{
    public record RobotRawState
    {
        public string MovementState { get; set; }
        public string[] Coordinates { get; set; }
    }
}
