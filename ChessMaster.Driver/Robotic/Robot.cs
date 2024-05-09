using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Events;
using ChessMaster.RobotDriver.SerialResponse;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class Robot : RobotBase
{
    private Queue<SerialCommand> commandQueue;
    private bool isBeingExecuted = true;
    private readonly SerialCommandFactory commands;
    private readonly ISerialDriver driver;
    private Vector3 origin;
    private const float safePadding = 5f;
    private RobotRawState state;
    private bool hasBeenInitialized = false;
    private SemaphoreSlim semaphore;

    public override Vector3 Origin
    {
        get => new Vector3(-origin.X - safePadding, -origin.Y - safePadding, -origin.Z - safePadding);
    }

    public Robot(string portName)
    {
        commands = new SerialCommandFactory();
        driver = new SerialDriver(portName);
        commandQueue = new Queue<SerialCommand>();
        semaphore = new SemaphoreSlim(1);
    }

    public override void Initialize()
    {
        if (!hasBeenInitialized)
        {
            bool problematicPosition = false;
            semaphore.Wait();
            try
            {
                driver.Initialize();
                origin = driver.GetOrigin();
                if (origin.X < 0 || origin.Y < 0 || origin.Z < 0)
                {
                    problematicPosition = true;
                }
                driver.SetMovementType(commands.LinearMovement());
                state = driver.GetRawState();
                hasBeenInitialized = true;
            }
            catch (Exception ex)
            {
                semaphore.Release();
                HandleRestartRequired();
                return;
            }

            if (driver.HomingRequired || problematicPosition)
            {
                semaphore.Release();
                HandleHomingRequired();
                problematicPosition = false;
            }
            else
            {
                semaphore.Release();
                HandleInitialized();
            }
        }
    }
    public override void Home()
    {
        semaphore.Wait();

        //TODO - to je nejak spatne
        semaphore.Release();

        isBeingExecuted = true;
        semaphore.Release();
        bool isIdle = false;
        while (!isIdle)
        {
            state = driver.GetRawState();
            isIdle = state.MovementState == MovementState.Idle.ToString();

            if (!isIdle)
            {
                Task.Delay(10);
            }
            else
            {
                driver.Home();
            }
        }
        hasBeenInitialized = true;
        HandleOkReponse();
    }
    public override void Reset()
    {
        driver.Reset();
    }
    public override void Pause()
    {
        SendCommandAtLastCompletion(commands.Pause());
    }
    public override void Resume()
    {
        SendCommandAtLastCompletion(commands.Resume());
    }

    public override RobotState GetState()
    {
        if (!hasBeenInitialized)
        {
            return new RobotState(MovementState.Unknown, RobotResponse.NotInitialized, 0, 0, 0);
        }

        MovementState stateResult = state.MovementState.ToMovementState();

        var x = float.Parse(state.Coordinates[0], System.Globalization.CultureInfo.InvariantCulture) - origin.X;
        var y = float.Parse(state.Coordinates[1], System.Globalization.CultureInfo.InvariantCulture) - origin.Y;
        var z = float.Parse(state.Coordinates[2], System.Globalization.CultureInfo.InvariantCulture) - origin.Z;

        return new RobotState(stateResult, RobotResponse.Ok, x, y, z);
    }
    public override void ScheduleCommands(Queue<RobotCommand> commands)
    {
        semaphore.Wait();
        if (driver.HomingRequired)
        {
            semaphore.Release();
            HandleHomingRequired();
        }
        if (!hasBeenInitialized)
        {
            semaphore.Release();
            HandleNotInitialized();
        }
        if (commandQueue.Count > 0 || isBeingExecuted)
        {
            semaphore.Release();
            HandleAlreadyExecuting();
        }

        while (commands.Count > 0)
        {
            commandQueue.Enqueue(commands.Dequeue().GetSerialCommand());
        }

        semaphore.Release();

        try
        {
            while (commandQueue.Count > 0)
            {
                semaphore.Wait();

                var serialCommand = commandQueue.Dequeue();
                isBeingExecuted = true;
                semaphore.Release();

                SendCommandAtLastCompletion(serialCommand);

            }
            bool isIdle = false;
            while (!isIdle)
            {
                state = driver.GetRawState();
                isIdle = state.MovementState == MovementState.Idle.ToString();

                if (!isIdle)
                {
                    Task.Delay(10);
                }
                else
                {
                    HandleOkReponse();
                }
            }
        }
        catch (Exception ex)
        {
            semaphore.Release();
            HandleRestartRequired();
        }
    }

    protected override void HandleInitialized()
    {
        var resultState = PrepareSuccessResponse();
        resultState.RobotResponse = RobotResponse.Initialized;
        Task.Run(() => OnInitialized(new RobotEventArgs(success: true, resultState)));
    }
    protected override void HandleOkReponse()
    {
        var resultState = PrepareSuccessResponse();
        resultState.RobotResponse = RobotResponse.Ok;
        Task.Run(() => OnCommandsSucceded(new RobotEventArgs(success: true, resultState)));
    }

    private void SendCommandAtLastCompletion(SerialCommand command)
    {
        bool isIdle = false;
        while (!isIdle)
        {
            state = driver.GetRawState();
            isIdle = state.MovementState == MovementState.Idle.ToString();

            if (!isIdle)
            {
                Task.Delay(10);
            }
            else
            {
                driver.TrySendCommand(command);
            }
        }
    }
    private RobotState PrepareSuccessResponse()
    {
        state = driver.GetRawState();
        var isIdle = state.MovementState == MovementState.Idle.ToString();
        while (!isIdle)
        {
            Task.Delay(10);
            state = driver.GetRawState();
            isIdle = state.MovementState == MovementState.Idle.ToString();
        }
        semaphore.Wait();
        isBeingExecuted = false;
        semaphore.Release();
        GetState();
        return GetState();
    }
}