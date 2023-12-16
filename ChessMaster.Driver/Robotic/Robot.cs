using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.SerialDriver;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic;

public class Robot : IRobot
{
    private Queue<SerialCommand> commandQueue;
    private readonly SerialCommandFactory commands;
    private readonly ISerialDriver driver;
    private Vector3 origin;
    private const float safePadding = 5f;
    private Mutex mutex;
    private bool IsConfiguration = true;
    public Robot(ISerialDriver robotDriver)
    {
        commands = new SerialCommandFactory();
        driver = robotDriver;
        commandQueue = new Queue<SerialCommand>();
        mutex = new Mutex(false);
    }
    public CommandsCompletedEvent CommandsExecuted { get; set; }
    public async Task Initialize()
    {
        await driver.Initialize();
        
        origin = driver.GetOrigin();
        await driver.SetMovementType(commands.LinearMovement());
    }
    public async Task<RobotState> GetState()
    {
        mutex.WaitOne();
        var rawState = await driver.GetRawState();
        mutex.ReleaseMutex();

        MovementState state = rawState.MovementState.ToMovementState();

        var x = float.Parse(rawState.Coordinates[0], System.Globalization.CultureInfo.InvariantCulture) - origin.X;
        var y = float.Parse(rawState.Coordinates[1], System.Globalization.CultureInfo.InvariantCulture) - origin.Y;
        var z = float.Parse(rawState.Coordinates[2], System.Globalization.CultureInfo.InvariantCulture) - origin.Z;

        return new RobotState(state, x, y, z);
    }
    protected virtual void OnCommandsExecuted(RobotEventArgs e)
    {
        CommandsExecuted?.Invoke(this, e);
    }
    public Vector3 Limits
    {
        get => new Vector3(-origin.X - safePadding, -origin.Y - safePadding, -origin.Z - safePadding);
    }
    public void Reset()
    {
        driver.Reset();
    }
    public void Pause()
    {
        mutex.WaitOne();
        var task = SendCommandAtLastCompletion(commands.Pause());
        task.Wait();
        mutex.ReleaseMutex();
    }
    public void Resume()
    {
        mutex.WaitOne();
        var task = SendCommandAtLastCompletion(commands.Resume());
        task.Wait();
        mutex.ReleaseMutex();
    }
    private async Task SendCommandAtLastCompletion(SerialCommand command)
    {
        bool isIdle = false;
        while (!isIdle)
        {
            var currentState = await driver.GetRawState();
            isIdle = currentState.MovementState == MovementState.Idle.ToString();

            if (!isIdle)
            {
                await Task.Delay(10);
            }
            else
            {
                await driver.SendCommand(command);
            }
        }
    }
    private void HandleFinishedCommands()
    {
        var currentState = driver.GetRawState().Result;
        var isIdle = currentState.MovementState == MovementState.Idle.ToString();
        while (!isIdle)
        {
            Task.Delay(10);
            currentState = driver.GetRawState().Result;
            isIdle = currentState.MovementState == MovementState.Idle.ToString();
        }
        OnCommandsExecuted(new RobotEventArgs(success: true, new RobotState()));
    }
    public bool TryScheduleCommands(Queue<RobotCommand> commands)
    {
        if (commandQueue.Count > 0)
        {
            return false;
        }

        while (commands.Count > 0)
        {
            commandQueue.Enqueue(commands.Dequeue().GetSerialCommand());
        }
        while (commandQueue.Count > 0)
        {
            mutex.WaitOne();

            var command = commandQueue.Dequeue();
            var task = SendCommandAtLastCompletion(command);
            task.Wait();

            mutex.ReleaseMutex();
        }

        HandleFinishedCommands();
        return true;
    }
    public bool TryScheduleConfigurationCommand(RobotCommand command)
    {
        if (!IsConfiguration)
        {
            return false;
        }

        if (commandQueue.Count > 0)
        {
            mutex.WaitOne();

            var serialCommand = commandQueue.Dequeue();
            var task = SendCommandAtLastCompletion(serialCommand);
            task.Wait();

            mutex.ReleaseMutex();
        }

        HandleFinishedCommands();

        return true;
    }
}