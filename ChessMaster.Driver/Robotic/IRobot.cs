using System.Numerics;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.RobotDriver.Robotic;

public interface IRobot
{
    CommandsCompletedEvent? CommandsSucceeded { get; set; }
    CommandsCompletedEvent? Initialized { get; set; }
    CommandsCompletedEvent? NotInitialized { get; set; }
    CommandsCompletedEvent? CommandsFinished { get; set; }
    CommandsCompletedEvent? HomingRequired { get; set; }
    CommandsCompletedEvent? RestartRequired { get; set; }
    RobotPausedEvent? Paused { get; set; }

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