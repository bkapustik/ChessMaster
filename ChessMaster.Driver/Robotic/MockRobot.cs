using ChessMaster.RobotDriver.State;
using System.Numerics;
using System.Threading;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobot : IRobot
{
    private bool running = false;
    private bool initialized = false;
    private bool homingRequired = false;
    private bool restartRequired = false;

    private Vector3 position = new Vector3(0f, 0f, 0f);
    private Vector3 displayedPosition = new Vector3(0f, 0f, 0f);

    private float originX = -490f, originY = -820f, originZ = -200f;
    public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }

    public CommandsCompletedEvent CommandsSucceeded { get; set; }
    public CommandsCompletedEvent Initialized { get; set; }
    public CommandsCompletedEvent NotInitialized { get; set; }
    public CommandsCompletedEvent CommandsFinished { get; set; }
    public CommandsCompletedEvent HomingRequired { get; set; }
    public CommandsCompletedEvent RestartRequired { get; set; }

    public void ScheduleCommands(Queue<RobotCommand> commands)
    {
        if (!initialized)
        {
            HandleNotInitialized();
        }
        else if (homingRequired)
        {
            HandleHomingRequired();
        }
        else if (restartRequired)
        {
            HandleRestartRequired();
        }
        else
        {
            HandleOkReponse();
        }
    } 

    public bool IsAtDesired(Vector3 desired, RobotState state)
    {
        return true;
    }
    public RobotState GetState()
    {
        displayedPosition.X = (displayedPosition.X + 1) % 1000;
        if (displayedPosition.X % 2 == 1)
        {
            displayedPosition.Y = (displayedPosition.Y + 1) % 1000;
        }
        if (displayedPosition.Y % 2 == 1)
        {
            displayedPosition.Z = (displayedPosition.Z + 1) % 1000;
        }
        return new RobotState(MovementState.Idle, RobotResponse.Ok,displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
    }
    public void Pause() { }
    public void Resume() { }
    public void Initialize()
    {
        var diceThrow = new Random(3);

        if (diceThrow.Next(1,4) == 1)
        {
            initialized = true;
            HandleInitialized();
        }
        if (diceThrow.Next(1,4) == 2)
        {
            homingRequired = true;
            HandleHomingRequired();
        }
        else
        {
            restartRequired = true;
            HandleRestartRequired();
        }
    }
    public void Reset() { }
    public void Home()
    {
        if (homingRequired)
        {
            homingRequired = false;
            HandleOkReponse();
        }
        if (restartRequired)
        {
            HandleRestartRequired();
        }
        else if (!initialized)
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
        Task.Run(() => OnInitialized(new RobotEventArgs(false, new RobotState(RobotResponse.HomingRequired))));
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
