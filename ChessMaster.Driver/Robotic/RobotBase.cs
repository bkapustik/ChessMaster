using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public abstract class RobotBase : IRobot
{
    public RobotStateEvents Events { get; protected set; } = new();
    public abstract Vector3 Origin { get; }
    public abstract void ScheduleCommands(Queue<RobotCommand> commands);
    public abstract void Initialize();
    public abstract void Home();
    public abstract void Reset();
    public abstract void Pause();
    public abstract void Resume();
    public abstract RobotState GetState();
    public bool IsAtDesired(Vector3 desired)
    {
        float dx = desired.X - GetState().Position.X;
        float dy = desired.Y - GetState().Position.Y;

        return Math.Abs(dx) <= 0.5 && Math.Abs(dy) <= 0.5;
    }

    protected abstract void HandleOkReponse();
    protected abstract void HandleInitialized();

    protected void OnCommandsSucceded(RobotEventArgs e)
    {
        Events.CommandsSucceeded?.Invoke(this, e);
    }
    protected void OnInitialized(RobotEventArgs e)
    {
        Events.Initialized?.Invoke(this, e);
    }
    protected void OnNotInitialized(RobotEventArgs e)
    {
        Events.NotInitialized?.Invoke(this, e);
    }
    protected void OnAlreadyExecuting(RobotEventArgs e)
    {
        Events.AlreadyExecuting?.Invoke(this, e);
    }
    protected void OnHomingRequired(RobotEventArgs e)
    {
        Events.HomingRequired?.Invoke(this, e);
    }
    protected void OnRestartRequired(RobotEventArgs e)
    {
        Events.RestartRequired?.Invoke(this, e);
    }

    protected void HandleAlreadyExecuting()
    {
        var resultState = GetState();
        Task.Run(() => OnAlreadyExecuting(new RobotEventArgs(success: false, resultState)));
    }
    protected void HandleNotInitialized()
    {
        var resultState = new RobotState(MovementState.Unknown, RobotResponse.NotInitialized, 0, 0, 0);
        Task.Run(() => OnNotInitialized(new RobotEventArgs(success: false, resultState)));
    }
    protected void HandleHomingRequired()
    {
        var state = GetState();
        var resultState = new RobotState(MovementState.Unknown, RobotResponse.HomingRequired, state.Position);
        Task.Run(() => OnHomingRequired(new RobotEventArgs(success: false, resultState)));
    }
    protected void HandleRestartRequired()
    {
        var state = GetState();
        var resultState = new RobotState(MovementState.Unknown, RobotResponse.UnknownError, state.Position);
        Task.Run(() => OnRestartRequired(new RobotEventArgs(success: false, resultState)));
    }
}
