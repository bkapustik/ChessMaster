using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobot : IRobot
{
    protected RobotResponse SetupState { get; set; } = RobotResponse.NotInitialized;
    protected MovementState MovementState { get; set; } = MovementState.Unknown;

    private SemaphoreSlim semaphore;
    private bool isPaused = false;
    protected Vector3 displayedPosition = new Vector3(0f, 0f, 0f);

    private float originX = 200f, originY = 200f, originZ = 200f;
    public Vector3 Origin { get { return new Vector3(originX, originY, originZ); } }


    public CommandsCompletedEvent? CommandsSucceeded { get; set; }
    public CommandsCompletedEvent? Initialized { get; set; }
    public CommandsCompletedEvent? NotInitialized { get; set; }
    public CommandsCompletedEvent? CommandsFinished { get; set; }
    public CommandsCompletedEvent? HomingRequired { get; set; }
    public CommandsCompletedEvent? RestartRequired { get; set; }
    public RobotPausedEvent? Paused { get; set; }

    public MockRobot()
    {
        semaphore = new SemaphoreSlim(1);
    }

    public void ScheduleCommands(Queue<RobotCommand> commands)
    {
        if (SetupState == RobotResponse.NotInitialized)
        {
            HandleNotInitialized();
        }
        else if (SetupState == RobotResponse.HomingRequired)
        {
            HandleHomingRequired();
        }
        else if (SetupState == RobotResponse.UnknownError)
        {
            HandleRestartRequired();
        }
        else if (SetupState != RobotResponse.AlreadyExecuting && MovementState == MovementState.Idle)
        {
            ExecuteCommands(commands);
        }
    }
    public bool IsAtDesired(Vector3 desired)
    {
        if (SetupState == RobotResponse.AlreadyExecuting)
        {
            return false;
        }
        float dx = desired.X - GetState().Position.X;
        float dy = desired.Y - GetState().Position.Y;

        return Math.Abs(dx) <= 0.5 && Math.Abs(dy) <= 0.5;
    }
    public RobotState GetState()
    {
        return new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
    }
    public void Pause()
    {
        semaphore.Wait();
        isPaused = true;
        semaphore.Release();
    }
    public void Resume()
    {
        semaphore.Wait();
        isPaused = false;
        semaphore.Release();
    }
    public virtual void Initialize()
    {
        var diceThrow = new Random().Next(1, 15);

        if (diceThrow <= 2)
        {
            semaphore.Wait();
            SetupState = RobotResponse.HomingRequired;
            MovementState = MovementState.Idle;
            semaphore.Release();
            HandleHomingRequired();
        }
        else if (diceThrow == 3)
        {
            semaphore.Wait();
            SetupState = RobotResponse.UnknownError;
            MovementState = MovementState.Unknown;
            semaphore.Release();
            HandleRestartRequired();
        }
        else
        {
            semaphore.Wait();
            SetupState = RobotResponse.Initialized;
            MovementState = MovementState.Idle;
            semaphore.Release();
            HandleInitialized();
        }
    }
    public void Reset() { }
    public void Home()
    {
        semaphore.Wait();
        if (SetupState == RobotResponse.HomingRequired || SetupState == RobotResponse.Ok || SetupState == RobotResponse.Initialized)
        {
            SetupState = RobotResponse.AlreadyExecuting;
            MovementState = MovementState.Running;
            semaphore.Release();
            var commands = new Queue<RobotCommand>();
            commands.Enqueue(new MoveCommand(originX, originY, originZ));
            ExecuteCommands(commands);
        }
        else if (SetupState == RobotResponse.UnknownError)
        {
            HandleRestartRequired();
        }
        else if (SetupState == RobotResponse.NotInitialized)
        {
            HandleNotInitialized();
        }
    }

    protected void OnCommandsSucceded(RobotEventArgs e)
    {
        CommandsSucceeded?.Invoke(this, e);
    }
    protected void OnInitialized(RobotEventArgs e)
    {
        Initialized?.Invoke(this, e);
    }
    protected void OnNotInitialized(RobotEventArgs e)
    {
        NotInitialized?.Invoke(this, e);
    }
    protected void OnCommandsFinished(RobotEventArgs e)
    {
        CommandsFinished?.Invoke(this, e);
    }
    protected void OnHomingRequired(RobotEventArgs e)
    {
        HomingRequired?.Invoke(this, e);
    }
    protected void OnRestartRequired(RobotEventArgs e)
    {
        RestartRequired?.Invoke(this, e);
    }
    protected void HandleFinishedCommands(RobotResponse robotResponse)
    {
        var resultState = new RobotState(MovementState.Idle, RobotResponse.Ok, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
        Task.Run(() => OnCommandsFinished(new RobotEventArgs(success: false, resultState)));
    }
    protected virtual void HandleInitialized()
    {
        Task.Run(() => HandleHomingRequired());
    }
    protected void HandleNotInitialized()
    {
        var resultState = new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z); ;
        Task.Run(() => OnNotInitialized(new RobotEventArgs(success: false, resultState)));
    }
    protected void HandleOkReponse()
    {
        Task.Run(() => OnCommandsSucceded(new RobotEventArgs(success: true, new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z))));
    }
    private void HandleHomingRequired()
    {
        var resultState = new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
        Task.Run(() => OnHomingRequired(new RobotEventArgs(success: false, resultState)));
    }
    private void HandleRestartRequired()
    {
        var resultState = new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
        Task.Run(() => OnRestartRequired(new RobotEventArgs(success: false, resultState)));
    }
    private void ExecuteCommands(Queue<RobotCommand> commands)
    {
        SetupState = RobotResponse.AlreadyExecuting;
        MovementState = MovementState.Running;

        foreach (RobotCommand command in commands)
        {
            if (command is MoveCommand)
            {
                var moveCommand = (MoveCommand)command;

                var positionDifferenceVector = new Vector3(
                    moveCommand.X - displayedPosition.X,
                    moveCommand.Y - displayedPosition.Y,
                    moveCommand.Z - displayedPosition.Z
                );

                int positionVectorDenominator = positionDifferenceVector.X > 50 || positionDifferenceVector.Y > 50 ? 100 : 10;

                var partOfPositionDifferenceVector = positionDifferenceVector / (float)positionVectorDenominator;

                int movePartsLeft = positionVectorDenominator;

                while (movePartsLeft > 0)
                {
                    semaphore.Wait();
                    if (!isPaused)
                    {
                        displayedPosition += partOfPositionDifferenceVector;
                        movePartsLeft--;
                    }
                    semaphore.Release();
                    Thread.Sleep(5);
                }
            }
        }

        SetupState = RobotResponse.Ok;
        MovementState = MovementState.Idle;
        HandleOkReponse();
    }
}
