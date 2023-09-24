using System.Numerics;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Infrastructure
{
    public interface IRobotDriver
    {
        Vector3 Limits { get; }

        Task Initialize();

        Task Reset();

        Task Home();

        Task Move(float x, float y, float z);

        Task MoveXY(float x, float y);

        Task MoveX(float x);

        Task MoveY(float y);

        Task MoveZ(float z);

        Task OpenGrip();

        Task CloseGrip();

        Task<RobotState> GetState();

        Task Pause();

        Task Resume();

        Task Stop();
    }
}
