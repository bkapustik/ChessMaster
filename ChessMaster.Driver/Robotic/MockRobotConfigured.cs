using ChessMaster.RobotDriver.Robotic.Events;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobotConfigured : IRobot
{
    private RobotResponse state = RobotResponse.NotInitialized;

    private Vector3 position = new Vector3(0f, 0f, 0f);
    private Vector3 displayedPosition = new Vector3(0f, 0f, 0f);

    private float originX = -490f, originY = -820f, originZ = -200f;
    public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }

    public CommandsCompletedEvent? CommandsSucceeded { get; set; }
    public CommandsCompletedEvent? Initialized { get; set; }
    public CommandsCompletedEvent? NotInitialized { get; set; }
    public CommandsCompletedEvent? CommandsFinished { get; set; }
    public CommandsCompletedEvent? HomingRequired { get; set; }
    public CommandsCompletedEvent? RestartRequired { get; set; }

    public void ScheduleCommands(Queue<RobotCommand> commands)
    {
        if (state == RobotResponse.NotInitialized)
        {
            HandleNotInitialized();
        }
        else if (state == RobotResponse.HomingRequired)
        {
            HandleHomingRequired();
        }
        else if (state == RobotResponse.UnknownError)
        {
            HandleRestartRequired();
        }
        else
        {
            foreach (RobotCommand command in commands)
            {
                if (command is MoveCommand)
                {
                    var moveCommand = (MoveCommand)command;
                    displayedPosition.X = moveCommand.X;
                    displayedPosition.Y = moveCommand.Y;
                    displayedPosition.Z = moveCommand.Z;
                }
            }

            HandleOkReponse();
        }
    }
    public bool IsAtDesired(Vector3 desired, RobotState state)
    {
        return desired == state.Position;
    }
    public RobotState GetState()
    {
        return new RobotState(MovementState.Idle, RobotResponse.Ok, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
    }
    public void Pause() { }
    public void Resume() { }
    public void Initialize()
    {
        state = RobotResponse.Initialized;
        HandleInitialized();
    }
    public void Reset() { }
    public void Home()
    {
        if (state == RobotResponse.HomingRequired)
        {
            state = RobotResponse.Ok;
            HandleOkReponse();
        }
        if (state == RobotResponse.UnknownError)
        {
            HandleRestartRequired();
        }
        else if (state == RobotResponse.NotInitialized)
        {
            HandleNotInitialized();
        }
        else
        {
            HandleOkReponse();
        }
    }

    private void OnCommandsSucceded(RobotEventArgs e)
    {
        CommandsSucceeded?.Invoke(this, e);
    }
    private void OnInitialized(RobotEventArgs e)
    {
        Initialized?.Invoke(this, e);
    }
    private void OnNotInitialized(RobotEventArgs e)
    {
        NotInitialized?.Invoke(this, e);
    }
    private void OnCommandsFinished(RobotEventArgs e)
    {
        CommandsFinished?.Invoke(this, e);
    }
    private void OnHomingRequired(RobotEventArgs e)
    {
        HomingRequired?.Invoke(this, e);
    }
    private void OnRestartRequired(RobotEventArgs e)
    {
        RestartRequired?.Invoke(this, e);
    }
    private void HandleFinishedCommands(RobotResponse robotResponse)
    {
        var resultState = new RobotState(robotResponse);
        Task.Run(() => OnCommandsFinished(new RobotEventArgs(success: false, resultState)));
    }
    private void HandleInitialized()
    {
        Task.Run(() => HandleOkReponse());
    }
    private void HandleNotInitialized()
    {
        var resultState = new RobotState(RobotResponse.NotInitialized);
        Task.Run(() => OnNotInitialized(new RobotEventArgs(success: false, resultState)));
    }
    private void HandleOkReponse()
    {
        Task.Run(() => OnCommandsSucceded(new RobotEventArgs(success: true, new RobotState(RobotResponse.Ok))));
    }
    private void HandleHomingRequired()
    {
        var resultState = new RobotState(RobotResponse.HomingRequired);
        Task.Run(() => OnHomingRequired(new RobotEventArgs(success: false, resultState)));
    }
    private void HandleRestartRequired()
    {
        var resultState = new RobotState(RobotResponse.UnknownError);
        Task.Run(() => OnRestartRequired(new RobotEventArgs(success: false, resultState)));
    }
}
