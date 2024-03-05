using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class MockRobot : RobotBase
{
    protected RobotResponse SetupState { get; set; } = RobotResponse.NotInitialized;
    protected MovementState MovementState { get; set; } = MovementState.Unknown;

    private SemaphoreSlim semaphore;
    private bool isPaused = false;
    protected Vector3 displayedPosition = new Vector3(0f, 0f, 0f);

    private float originX = 200f, originY = 200f, originZ = 200f;
    public override Vector3 Origin { get { return new Vector3(originX, originY, originZ); } }

    public MockRobot()
    {
        semaphore = new SemaphoreSlim(1);
        Events = new RobotStateEvents();
    }

    public override void ScheduleCommands(Queue<RobotCommand> commands)
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
        else if (SetupState == RobotResponse.AlreadyExecuting)
        {
            HandleAlreadyExecuting();
        }
        else if (MovementState == MovementState.Idle)
        {
            ExecuteCommands(commands);
        }
    }
    public override RobotState GetState()
    {
        return new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z);
    }
    public override void Pause()
    {
        semaphore.Wait();
        isPaused = true;
        semaphore.Release();
    }
    public override void Resume()
    {
        semaphore.Wait();
        isPaused = false;
        semaphore.Release();
    }
    public override void Initialize()
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
    public override void Reset() { }
    public override void Home()
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

    protected override void HandleInitialized()
    {
        Task.Run(() => HandleHomingRequired());
    }
    protected override void HandleOkReponse()
    {
        Task.Run(() => OnCommandsSucceded(new RobotEventArgs(success: true, new RobotState(MovementState, SetupState, displayedPosition.X, displayedPosition.Y, displayedPosition.Z))));
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

                int positionVectorDenominator = Math.Abs(positionDifferenceVector.X) > 50 || Math.Abs(positionDifferenceVector.Y) > 50 ? 100 : 10;
                positionVectorDenominator = 5;
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
