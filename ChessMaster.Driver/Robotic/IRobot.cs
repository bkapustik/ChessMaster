using System.Numerics;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Robotic;

public class RobotStateEvents
{
    public CommandsCompletedEvent? CommandsSucceeded { get; set; }
    public CommandsCompletedEvent? Initialized { get; set; }
    public CommandsCompletedEvent? NotInitialized { get; set; }
    public CommandsCompletedEvent? AlreadyExecuting { get; set; }
    public CommandsCompletedEvent? HomingRequired { get; set; }
    public CommandsCompletedEvent? RestartRequired { get; set; }
    public RobotPausedEvent? Paused { get; set; }
}

public interface IRobot
{
    RobotStateEvents Events { get; }
    Vector3 Origin { get; }
    void Initialize();
    RobotState GetState();
    void ScheduleCommands(Queue<RobotCommand> commands);
    void Reset();
    void Pause();
    void Resume();
    void Home();
    bool IsAtDesired(Vector3 desired);
}

public class RobotException : Exception
{
    public RobotException()
    {

    }

    public RobotException(string message)
        : base(message)
    {

    }

    public RobotException(string message, Exception inner)
        : base(message, inner)
    {

    }
}