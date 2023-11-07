using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.SerialResponse;
using System.Numerics;

namespace ChessMaster.RobotDriver.Driver
{
    public interface ISerialDriver
    {
        Task Initialize();
        void ScheduleCommand(string command);
        Task Reset();
        Task<RobotRawState> GetRawState();
        Vector3 GetOrigin();
        Task SetMovementType(string movementCommand);
        CommandsCompletedEvent CommandsExecuted { get; set; }
    }
}
