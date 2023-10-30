using System.Numerics;
using ChessMaster.Robot.State;

namespace ChessMaster.Robot.Robot
{
    public delegate void CommandsCompletedEvent(object? o, RobotEventArgs e);

    public interface IRobot
    {
        void SubscribeToCommandsCompletion(CommandsCompletedEvent e);
        Vector3 Limits { get; }
        Task Initialize();
        void Reset();
        void Home();
        void Move(float x, float y, float z);
        void Move(Vector3 targetPosition);
        void MoveXY(float x, float y);
        void MoveX(float x);
        void MoveY(float y);
        void MoveZ(float z);
        void OpenGrip();
        void CloseGrip();
        Task<RobotState> GetState();
        void Pause();
        void Resume();
        void Stop();
    }
}
