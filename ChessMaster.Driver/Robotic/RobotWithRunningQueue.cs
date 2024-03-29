﻿//using ChessMaster.RobotDriver.Driver;
//using ChessMaster.RobotDriver.Events;
//using ChessMaster.RobotDriver.SerialResponse;
//using ChessMaster.RobotDriver.State;
//using System.Numerics;

//namespace ChessMaster.RobotDriver.Robotic;
//public class RobotWithRunningQueue : IRobot
//{
//    private Queue<SerialCommand> commandQueue;
//    private bool isBeingExecuted = true;
//    private bool isPaused = false;
//    private readonly SerialCommandFactory commands;
//    private readonly ISerialDriver driver;
//    private Vector3 origin;
//    private const float safePadding = 5f;
//    private RobotRawState state;
//    private bool hasBeenInitialized = false;
//    private SemaphoreSlim semaphore;

//    public CommandsCompletedEvent? CommandsSucceeded { get; set; }
//    public CommandsCompletedEvent? Initialized { get; set; }
//    public CommandsCompletedEvent? NotInitialized { get; set; }
//    public CommandsCompletedEvent? AlreadyExecuting { get; set; }
//    public CommandsCompletedEvent? HomingRequired { get; set; }
//    public CommandsCompletedEvent? RestartRequired { get; set; }
//    public RobotPausedEvent? Paused { get; set; }

//    public Vector3 Origin
//    {
//        get => new Vector3(-origin.X - safePadding, -origin.Y - safePadding, -origin.Z - safePadding);
//    }

//    public RobotWithRunningQueue(string portName)
//    {
//        commands = new SerialCommandFactory();
//        driver = new SerialDriver(portName);
//        commandQueue = new Queue<SerialCommand>();
//        semaphore = new SemaphoreSlim(1);
//    }

//    public void Run()
//    {
//        while (!isPaused)
//        {
//            try
//            {
//                if (commandQueue.Count > 0)
//                {
//                    semaphore.Wait();

//                    var serialCommand = commandQueue.Dequeue();
//                    isBeingExecuted = true;
//                    semaphore.Release();

//                    SendCommandAtLastCompletion(serialCommand);
//                }
//                else
//                {
//                    Thread.Sleep(100);
//                }
//            }
//            catch (Exception ex)
//            {
//                semaphore.Release();
//                HandleRestartRequired();
//            }
//        }
//    }

//    public void Initialize()
//    {
//        if (!hasBeenInitialized)
//        {
//            bool problematicPosition = false;
//            semaphore.Wait();
            
//            try
//            {
//                driver.Initialize();
//                origin = driver.GetOrigin();
//                if (origin.X < 0 || origin.Y < 0 || origin.Z < 0)
//                {
//                    problematicPosition = true;
//                }
//                driver.SetMovementType(commands.LinearMovement());
//                state = driver.GetRawState();
//                hasBeenInitialized = true;
//            }
//            catch (Exception ex)
//            {
//                semaphore.Release();
//                HandleRestartRequired();
//                return;
//            }

//            if (driver.HomingRequired || problematicPosition)
//            {
//                semaphore.Release();
//                HandleHomingRequired();
//                problematicPosition = false;
//            }
//            else
//            {
//                semaphore.Release();
//                Task.Run(Run);
//                HandleInitialized();
//            }
//        }
//    }
//    public bool IsAtDesired(Vector3 desired)
//    {
//        float dx = desired.X - GetState().Position.X;
//        float dy = desired.Y - GetState().Position.Y;

//        return Math.Abs(dx) <= 0.5 && Math.Abs(dy) <= 0.5;
//    }
//    public void Home()
//    {
//        semaphore.Wait();

//        //TODO - to je nejak spatne
//        semaphore.Release();

//        isBeingExecuted = true;
//        semaphore.Release();
//        bool isIdle = false;
//        while (!isIdle)
//        {
//            state = driver.GetRawState();
//            isIdle = state.MovementState == MovementState.Idle.ToString();

//            if (!isIdle)
//            {
//                Task.Delay(10);
//            }
//            else
//            {
//                driver.Home();
//            }
//        }
//        if (!hasBeenInitialized)
//        {
//            hasBeenInitialized = true;
//            Task.Run(Run);
//        }
//        HandleOkReponse();
//    }
//    public void Reset()
//    {
//        isPaused = false;
//        commandQueue = new();
//        driver.Reset();
//    }
//    public void Pause()
//    {
//        semaphore.Wait();
//        isPaused = true;
//        semaphore.Release();
        
//        SendCommandAtLastCompletion(commands.Pause());
//    }
//    public void Resume()
//    {
//        semaphore.Wait();
//        isPaused = false;
//        semaphore.Release();

//        SendCommandAtLastCompletion(commands.Resume());
//    }

//    public RobotState GetState()
//    {
//        if (!hasBeenInitialized)
//        {
//            return new RobotState(MovementState.Unknown, RobotResponse.NotInitialized, 0, 0, 0);
//        }

//        MovementState stateResult = state.MovementState.ToMovementState();

//        var x = float.Parse(state.Coordinates[0], System.Globalization.CultureInfo.InvariantCulture) - origin.X;
//        var y = float.Parse(state.Coordinates[1], System.Globalization.CultureInfo.InvariantCulture) - origin.Y;
//        var z = float.Parse(state.Coordinates[2], System.Globalization.CultureInfo.InvariantCulture) - origin.Z;

//        return new RobotState(stateResult, RobotResponse.Ok, x, y, z);
//    }

//    public void ScheduleCommands(Queue<RobotCommand> commands)
//    {
//        semaphore.Wait();
//        if (isPaused)
//        {
//            semaphore.Release();
//            HandlePaused();
//        }
//        if (driver.HomingRequired)
//        {
//            semaphore.Release();
//            HandleHomingRequired();
//        }
//        if (!hasBeenInitialized)
//        {
//            semaphore.Release();
//            HandleNotInitialized();
//        }
//        if (commandQueue.Count > 0 || isBeingExecuted)
//        {
//            semaphore.Release();
//            HandleFinishedCommands(RobotResponse.AlreadyExecuting);
//        }

//        while (commands.Count > 0)
//        {
//            commandQueue.Enqueue(commands.Dequeue().GetSerialCommand());
//        }

//        semaphore.Release();
//    }

//    private void OnCommandsSucceded(RobotEventArgs e)
//    {
//        CommandsSucceeded?.Invoke(this, e);
//    }
//    private void OnInitialized(RobotEventArgs e)
//    {
//        Initialized?.Invoke(this, e);
//    }
//    private void OnNotInitialized(RobotEventArgs e)
//    {
//        NotInitialized?.Invoke(this, e);
//    }
//    private void OnCommandsFinished(RobotEventArgs e)
//    {
//        AlreadyExecuting?.Invoke(this, e);
//    }
//    private void OnHomingRequired(RobotEventArgs e)
//    {
//        HomingRequired?.Invoke(this, e);
//    }
//    private void OnRestartRequired(RobotEventArgs e)
//    {
//        RestartRequired?.Invoke(this, e);
//    }
//    private void OnPaused(RobotPausedEventArgs e)
//    {
//        Paused?.Invoke(this, e);
//    }
//    private void SendCommandAtLastCompletion(SerialCommand command)
//    {
//        bool isIdle = false;
//        while (!isIdle)
//        {
//            state = driver.GetRawState();
//            isIdle = state.MovementState == MovementState.Idle.ToString();

//            if (!isIdle)
//            {
//                Task.Delay(10);
//            }
//            else
//            {
//                driver.TrySendCommand(command);
//            }
//        }
//    }

//    private RobotState PrepareSuccessResponse()
//    {
//        state = driver.GetRawState();
//        var isIdle = state.MovementState == MovementState.Idle.ToString();
//        while (!isIdle)
//        {
//            Task.Delay(10);
//            state = driver.GetRawState();
//            isIdle = state.MovementState == MovementState.Idle.ToString();
//        }
//        semaphore.Wait();
//        isBeingExecuted = false;
//        semaphore.Release();
//        GetState();
//        return GetState();
//    }

//    private void HandlePaused()
//    {
//        Task.Run(() => OnPaused(new RobotPausedEventArgs()));
//    }
//    private void HandleFinishedCommands(RobotResponse robotResponse)
//    {
//        var resultState = GetState();
//        Task.Run(() => OnCommandsFinished(new RobotEventArgs(success: false, resultState)));
//    }
//    private void HandleInitialized()
//    {
//        var resultState = PrepareSuccessResponse();
//        resultState.RobotResponse = RobotResponse.Initialized;
//        Task.Run(() => OnInitialized(new RobotEventArgs(success: true, resultState)));
//    }
//    private void HandleNotInitialized()
//    {
//        var resultState = new RobotState(MovementState.Unknown, RobotResponse.NotInitialized, 0, 0, 0);
//        Task.Run(() => OnNotInitialized(new RobotEventArgs(success: false, resultState)));
//    }
//    private void HandleOkReponse()
//    {
//        var resultState = PrepareSuccessResponse();
//        resultState.RobotResponse = RobotResponse.Ok;
//        Task.Run(() => OnCommandsSucceded(new RobotEventArgs(success: true, resultState)));
//    }
//    private void HandleHomingRequired()
//    {
//        var state = GetState();
//        var resultState = new RobotState(MovementState.Unknown, RobotResponse.HomingRequired, state.Position);
//        Task.Run(() => OnHomingRequired(new RobotEventArgs(success: false, resultState)));
//    }
//    private void HandleRestartRequired()
//    {
//        var state = GetState();
//        var resultState = new RobotState(MovementState.Unknown, RobotResponse.UnknownError, state.Position);
//        Task.Run(() => OnRestartRequired(new RobotEventArgs(success: false, resultState)));
//    }
//}
