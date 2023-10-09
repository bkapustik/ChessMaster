using ChessMaster.CommandFactory;
using ChessMaster.Robot.SerialResponse;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Robot.Driver
{
    public interface ISerialDriver
    {
        Task Initialize();
        Task SendCommand(string command, long timeout = 5000);
        Task Reset();
        Task<RobotRawState> GetRawState();
        Vector3 GetOrigin();
        Task SetMovementType(string movementCommand);
    }
}
