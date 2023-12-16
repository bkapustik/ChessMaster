using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System.Numerics;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.Space.RobotSpace;

public class RobotSpace
{
    protected Space space;
    protected IRobot robot;

    private MoveableEntity? currentlyHeldEntity;
    private RobotState currentRobotState;
    private SpacePosition expectedPosition;
    public async Task InitializeAsync()
    {
        await robot.Initialize();
    }
    public void Initialize()
    { 
        robot.Initialize().Wait();
    }
    public void Initialize(IRobot robot)
    { 
        this.robot = robot;
        robot.Initialize().Wait();
    }
    protected void MoveEntityFromSourceToTarget(SpacePosition source, SpacePosition target)
    {
        TakeEntityFromPosition(source);

        MoveToCarryingPosition(target);

        MoveEntityToPosition(target);
    }
    public void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
    {
        robot.CommandsExecuted += e;
    }
    private void UpdateCurrentState(SpacePosition position)
    { 
        expectedPosition = position;
    }
    public async Task<RobotState> GetState() => await robot.GetState();
    private void TakeEntityFromPosition(SpacePosition position)
    {
        var commands = new Queue<RobotCommand>();
        var entity = space.SubSpaces[position.X,position.Y].Entity;

        var moves = GetBestTrajectory(position);

        while (moves.Count > 0)
        {
            var move = moves.Dequeue();

            commands.Enqueue(new MoveCommand(move));
        }

        commands.Enqueue(new OpenCommand());
        commands.Enqueue(new MoveCommand(entity!.GetHoldingPointVector()));
        commands.Enqueue(new CloseCommand());

        if (!robot.TryScheduleCommands(commands))
        {
            robot.CommandsExecuted += new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
            {
                robot.TryScheduleCommands(commands);
            });
        }

        currentlyHeldEntity = entity;
        space.SubSpaces[position.X, position.Y].Entity = null;
        UpdateCurrentState(position);
    }
    private void MoveEntityToPosition(SpacePosition targetPosition)
    {
        var commands = new Queue<RobotCommand>();   
        space.SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;

        currentlyHeldEntity = null;

        var moves = GetBestTrajectory(targetPosition);

        while (moves.Count > 0)
        {
            var move = moves.Dequeue();

            commands.Enqueue(new MoveCommand(move));
        }

        commands.Enqueue(new CloseCommand());

        if (!robot.TryScheduleCommands(commands))
        {
            robot.CommandsExecuted += new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
            {
                robot.TryScheduleCommands(commands);
            });
        }

        UpdateCurrentState(targetPosition);
    }
    private void MoveToCarryingPosition(SpacePosition spacePosition)
    {
        var carryingMove = GetBestCarryingPosition(spacePosition);

        var commands = new Queue<RobotCommand>();
        commands.Enqueue(new MoveCommand(carryingMove));
        
        if (!robot.TryScheduleCommands(commands))
        {
            robot.CommandsExecuted += new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
            {
                robot.TryScheduleCommands(commands);
            });
        }

        UpdateCurrentState(spacePosition);
    }
    private Queue<Vector3> GetBestTrajectory(SpacePosition targetPosition)
    {
        var currentCoordinations = space.SubSpaces[expectedPosition.X, expectedPosition.Y].Center.Value;
        var targetCoordinations = space.SubSpaces[targetPosition.X, targetPosition.Y].Center.Value;

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
    public void Move(Vector2 position)
    {
        throw new NotImplementedException();
    }
}
