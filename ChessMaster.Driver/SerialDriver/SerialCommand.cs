using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.RobotDriver.SerialDriver
{
    public class SerialCommand
    {
        public string Command { get; set; }
        public int Timeout { get; set; }
        public int? TimeToExecute { get; set; }

        public SerialCommand(string command, int? timeToExecute = null, int timeOut = 500000)
        {
            Command = command;
            Timeout = timeOut;
            TimeToExecute = timeToExecute;
        }
    }
}
