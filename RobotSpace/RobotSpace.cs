using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System.Numerics;
using ChessMaster.RobotDriver.State;
using ChessMaster.RobotDriver.Events;

namespace ChessMaster.Space.RobotSpace;

public class RobotSpace
{
    public RobotStateEvents? Events { get; protected set; }

    protected Space? space;
    protected IRobot? Driver { get; set; }

    private bool DriverInitialized { get; set; } = false;

    private MoveableEntity? currentlyHeldEntity;
    private SpacePosition expectedPosition;
    public Vector3 GetOrigin()
    {
        if (Driver == null || !DriverInitialized)
        {
            throw new RobotException("Robot is not initialized");
        }

        return Driver.Origin;
    }
    public void Initialize()
    {
        if (Driver == null)
        {
            throw new RobotException("Port Driver has not been chosen yet.");
        }
        Driver!.Initialize();
        DriverInitialized = true;
    }
    protected void MoveEntityFromSourceToTarget(SpacePosition source, SpacePosition target)
    {
        TakeEntityFromPosition(source);

        MoveToCarryingPosition(target);

        MoveEntityToPosition(target);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"> If <see cref="IRobot"/> has not been initialized.</exception>
    public void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
    {
        Driver!.Events.CommandsSucceeded += e;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns><see cref="RobotState"/></returns>
    /// <exception cref="ArgumentNullException"> If <see cref="IRobot"/> has not been initialized.</exception>
    public RobotState GetState() => Driver!.GetState();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <exception cref="ArgumentNullException">If <see cref="Space"/> or <see cref="IRobot"/> has not been initialized</exception>
    private void TakeEntityFromPosition(SpacePosition position)
    {
        var commands = new Queue<RobotCommand>();
        var entity = space!.SubSpaces[position.X, position.Y].Entity;

        var moves = GetBestTrajectory(position);

        while (moves.Count > 0)
        {
            var move = moves.Dequeue();

            commands.Enqueue(new MoveCommand(move));
        }

        commands.Enqueue(new OpenCommand());
        commands.Enqueue(new MoveCommand(entity!.GetHoldingPointVector()));
        commands.Enqueue(new CloseCommand());

        var state = GetState();

        if (state.RobotResponse != RobotResponse.Ok && state.RobotResponse != RobotResponse.Initialized)
        {
            throw new InvalidOperationException("Can not schedule commands before previously scheduled commands are executed");
        }

        Driver!.ScheduleCommands(commands);

        currentlyHeldEntity = entity;
        space.SubSpaces[position.X, position.Y].Entity = null;
    }

    private void MoveEntityToPosition(SpacePosition targetPosition)
    {
        //var commands = new Queue<RobotCommand>();
        //space.SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;

        //currentlyHeldEntity = null;

        //var moves = GetBestTrajectory(targetPosition);

        //while (moves.Count > 0)
        //{
        //    var move = moves.Dequeue();

        //    commands.Enqueue(new MoveCommand(move));
        //}

        //commands.Enqueue(new CloseCommand());

        //if (!robot.TryScheduleCommands(commands))
        //{
        //    robot.CommandsExecuted += new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        //    {
        //        robot.TryScheduleCommands(commands);
        //    });
        //}

        //UpdateCurrentState(targetPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spacePosition"></param>
    /// <exception cref="ArgumentNullException">If <see cref="Space"/> or <see cref="IRobot"/> has not been initialized</exception>
    private void MoveToCarryingPosition(SpacePosition spacePosition)
    {
        //var carryingMove = GetBestCarryingPosition(spacePosition);

        //var commands = new Queue<RobotCommand>();
        //commands.Enqueue(new MoveCommand(carryingMove));

        //if (!robot.TryScheduleCommands(commands))
        //{
        //    robot.CommandsExecuted += new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        //    {
        //        robot.TryScheduleCommands(commands);
        //    });
        //}

        //UpdateCurrentState(spacePosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private Queue<Vector3> GetBestTrajectory(SpacePosition targetPosition)
    {
        var currentCoordinations = space.SubSpaces[expectedPosition.X, expectedPosition.Y].Center2.Value;
        var targetCoordinations = space.SubSpaces[targetPosition.X, targetPosition.Y].Center2.Value;

        //var intersectedSpaceIndices = new List<int>();

        //for (int i = 0; i < spaces.Length; i++)
        //{ 
        //    if ()
        //}

        var result = new Queue<Vector3>();
        result.Enqueue(new Vector3(targetCoordinations.X, targetCoordinations.Y, 100));
        return result;

        //TODO implemenet
    }
    private Vector3 GetBestCarryingPosition(SpacePosition spacePosition)
    {
        return new Vector3();
        //TODO implemenet
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <exception cref="ArgumentNullException"> If <see cref="IRobot"/> has not been initialized.</exception>
    public void Move(Vector3 position)
    {
        var commands = new Queue<RobotCommand>();
        commands.Enqueue(new MoveCommand(position));
        Driver!.ScheduleCommands(commands);
    }

    public void Home()
    {
        if (Driver == null)
        {
            throw new NullReferenceException("Driver is null");
        }

        Driver!.Home();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="desired"></param>
    /// <exception cref="ArgumentNullException"> If <see cref="IRobot"/> has not been initialized.</exception>
    /// <returns></returns>
    public bool IsAtDesired(Vector3 desired) => Driver!.IsAtDesired(desired);
}
