using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.SerialDriver;
using ChessMaster.RobotDriver.SerialResponse;
using System.Numerics;

namespace ChessMaster.RobotDriver.Driver
{
    public interface ISerialDriver
    {
        Task Initialize();
        Task Reset();
        Task<RobotRawState> GetRawState();
        Vector3 GetOrigin();
        Task SetMovementType(SerialCommand movementCommand);
        Task SendCommand(SerialCommand command);
    }
}
