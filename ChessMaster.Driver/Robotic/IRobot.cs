using System.Numerics;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Robotic
{
    public delegate void CommandsCompletedEvent(object? o, RobotEventArgs e);

    public interface IRobot
    {
        CommandsCompletedEvent CommandsExecuted { get; set; }
        Vector3 Limits { get; }
        Task Initialize();
        Task<RobotState> GetState();
        
        bool TryScheduleCommands(Queue<RobotCommand> commands);
        bool TryScheduleConfigurationCommand(RobotCommand command);

        void Reset();
        void Pause();
        void Resume();
    }
}
