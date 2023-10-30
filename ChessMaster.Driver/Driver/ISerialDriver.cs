using ChessMaster.Robot.Robot;
using ChessMaster.Robot.SerialResponse;
using System.Numerics;

namespace ChessMaster.Robot.Driver
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
