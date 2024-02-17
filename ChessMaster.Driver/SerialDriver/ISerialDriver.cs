using ChessMaster.RobotDriver.SerialResponse;
using System.Numerics;

namespace ChessMaster.RobotDriver.Driver
{
    public interface ISerialDriver
    {
        bool HomingRequired { get; }
        void Initialize();
        void Reset();
        void SetMovementType(SerialCommand movementCommand);
        bool TrySendCommand(SerialCommand command);
        void Home();
        RobotRawState GetRawState();
        Vector3 GetOrigin();
    }
}
