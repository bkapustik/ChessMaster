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
    protected float TileWidth { get; set; }
    protected float SafePaddingBetweenFigures { get; set; }

    private bool DriverInitialized { get; set; } = false;

    private MoveableEntity? currentlyHeldEntity;
    private Vector3 expectedResultingPosition;

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
        expectedResultingPosition = GetState().Position;

        var commands = GetTakeEntityFromPositionCommands(source);
        var moveEntityCommands = GetMoveEntityToPositionCommands(target);
        while (moveEntityCommands.Count > 0)
        { 
            commands.Enqueue(moveEntityCommands.Dequeue());
        }

        Driver!.ScheduleCommands(commands);
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
    private Queue<RobotCommand> GetTakeEntityFromPositionCommands(SpacePosition position)
    {
        var commands = new Queue<RobotCommand>();
        var entity = space!.SubSpaces[position.Row, position.Column].Entity;

        commands.Enqueue(new OpenCommand());

        var moves = GetTrajectory(position);

        if (moves.Count > 0)
        {
            expectedResultingPosition = moves.Last();

            while (moves.Count > 0)
            {
                var move = moves.Dequeue();

                commands.Enqueue(new MoveCommand(move));
            }
        }

        commands.Enqueue(new CloseCommand());

        var state =GetState();

        if (state.RobotResponse != RobotResponse.Ok && state.RobotResponse != RobotResponse.Initialized)
        {
            throw new InvalidOperationException("Can not schedule commands before previously scheduled commands are executed");
        }

        currentlyHeldEntity = entity;
        space.SubSpaces[position.Row, position.Column].Entity = null;

        return commands;
    }

    private Queue<RobotCommand> GetMoveEntityToPositionCommands(SpacePosition targetPosition)
    {
        var commands = new Queue<RobotCommand>();

        float entityHeight = currentlyHeldEntity!.Height!.Value;
        space!.SubSpaces[targetPosition.Row, targetPosition.Column].Entity = currentlyHeldEntity;
        currentlyHeldEntity = null;

        var moves = GetTrajectory(targetPosition);

        if (moves.Count > 0)
        {
            expectedResultingPosition = moves.Last();

            while (moves.Count > 0)
            {
                var move = moves.Dequeue();

                commands.Enqueue(new MoveCommand(move));
            }
        }

        commands.Enqueue(new OpenCommand());
        commands.Enqueue(new MoveCommand(new Vector3(expectedResultingPosition.X, expectedResultingPosition.Y, SafePaddingBetweenFigures + entityHeight)));
        commands.Enqueue(new CloseCommand());

        expectedResultingPosition.Z = SafePaddingBetweenFigures + entityHeight;

        return commands;
    }

    // TODO Delete if not neccessary
    [Obsolete]
    private void MoveToCarryingPosition(SpacePosition targetPosition)
    {
        var commands = new Queue<RobotCommand>();

        var moves = GetTrajectory(targetPosition);

        if (moves.Count > 0)
        {
            expectedResultingPosition = moves.Last();

            while (moves.Count > 0)
            {
                var move = moves.Dequeue();

                commands.Enqueue(new MoveCommand(move));
            }
        }

        Driver!.ScheduleCommands(commands);
    }

    protected float GetCarryHeight(MoveableEntity? figure)
    {
        if (figure != null)
        {
            return SafePaddingBetweenFigures + figure.Center3!.Value.Z;
        }

        return SafePaddingBetweenFigures;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private Queue<Vector3> GetTrajectory(SpacePosition targetPosition)
    {
        var resultPoints = new List<Vector3>();

        float heightPadding = GetCarryHeight(currentlyHeldEntity);

        SpacePosition currentPosition = new SpacePosition((int)(expectedResultingPosition.Y / TileWidth), (int)(expectedResultingPosition.X / TileWidth));

        List<SpacePosition> intersectedTiles = ComputeLineSupercoverSet(currentPosition, targetPosition);

        float sourcePointY = expectedResultingPosition.Y;
        float sourcePointX = expectedResultingPosition.X;
        float targetPointY = (targetPosition.Row * TileWidth) + (TileWidth / 2);
        float targetPointX = (targetPosition.Column * TileWidth) + (TileWidth / 2);

        if (intersectedTiles.Count > 0)
        {
            float mLineGradient = 0;
            float cHeight = 0;

            if (targetPointX != sourcePointX)
            {
                mLineGradient = (targetPointY - sourcePointY) / (targetPointX - sourcePointX);
                cHeight = -(mLineGradient * sourcePointX) + sourcePointY;
            }

            int dy = Math.Abs(targetPosition.Row - currentPosition.Row);
            int dx = Math.Abs(targetPosition.Column - currentPosition.Column);

            IEnumerable<IGrouping<int, SpacePosition>> tilesGrouped;

            if (dy > dx)
            {
                tilesGrouped = intersectedTiles.GroupBy(tile => tile.Row);
            }
            else
            {
                tilesGrouped = intersectedTiles.GroupBy(tile => tile.Column);
            }

            List<GroupCoordination> groupCoordinations = tilesGrouped.Select(group => group
                .MaxBy(tile => space!.SubSpaces[tile.Row, tile.Column].Entity?.Height ?? 0))
                .Select(tile => new GroupCoordination(tile,
                    space!.SubSpaces[tile.Row, tile.Column].Entity?.Height ?? 0)
                ).ToList();

            foreach (var group in groupCoordinations)
            {
                group.ComputeTileIntersectionTowardsTarget(dy, dx, TileWidth, cHeight, mLineGradient);
            }
            
            int maximumHeightIndex = GroupCoordination.GetLastMaximumHeightIndex(groupCoordinations);
            float maximumHeight = groupCoordinations[maximumHeightIndex].Height;

            float previousHeight = 0;
            for (int i = 0; i <= maximumHeightIndex; i++)
            {
                var group = groupCoordinations[i];
                if (group.Height > previousHeight)
                {
                    previousHeight = group.Height;
                    resultPoints.Add(new Vector3(group.RealX, group.RealY, group.Height));
                }
                else if (group.Height == maximumHeight && i < maximumHeightIndex)
                {
                    resultPoints.Add(new Vector3(groupCoordinations[maximumHeightIndex].RealX, groupCoordinations[maximumHeightIndex].RealY, maximumHeight));
                }
            }

            if (groupCoordinations[groupCoordinations.Count - 1].Height != maximumHeight)
            {
                var last = groupCoordinations[groupCoordinations.Count - 1];

                previousHeight = last.Height;

                var reverseAscendFromEndToMaxHeight = new List<Vector3>()
                {
                    new Vector3(last.RealX, last.RealY, last.Height)
                };

                for (int i = groupCoordinations.Count - 1; i > maximumHeightIndex; i--)
                {
                    var group = groupCoordinations[i];
                    if (group.Height > previousHeight)
                    {
                        previousHeight = group.Height;
                        reverseAscendFromEndToMaxHeight.Add(new Vector3(group.RealX, group.RealY, group.Height));
                    }
                }

                for (int i = reverseAscendFromEndToMaxHeight.Count - 1; i >= 0; i--)
                {
                    resultPoints.Add(reverseAscendFromEndToMaxHeight[i]);
                }
            }
        }

        var result = new Queue<Vector3>();

        for (int i = 0; i < resultPoints.Count; i++)
        {
            var point = resultPoints[i];
            point.Z += heightPadding;
            result.Enqueue(point);
        }

        float targetZ = 0;

        if (currentlyHeldEntity == null)
        {
            targetZ = space!.SubSpaces[targetPosition.Row, targetPosition.Column].Entity!.Center3!.Value.Z;
        }
        else
        { 
            targetZ = currentlyHeldEntity.Center3!.Value.Z;
        }

        result.Enqueue(new Vector3(targetPointX, targetPointY, targetZ));

        return result;
    }

    private class GroupCoordination
    {
        public float Height { get; set; }
        public SpacePosition Position { get; set; }
        public float RealX { get; set; }
        public float RealY { get; set; }

        public GroupCoordination(SpacePosition position, float height)
        {
            Height = height;
            Position = position;
        }

        public void ComputeTileIntersectionTowardsTarget(int dy, int dx, float tileWidth, float cHeight, float mLineGradient)
        {
            if (dx == 0)
            {
                RealX = Position.Column * tileWidth + (tileWidth / 2);
                RealY = Position.Row * tileWidth + (tileWidth / 2);
            }
            else if (dy > dx)
            {
                RealY = Position.Row * tileWidth + (tileWidth / 2);
                RealX = (RealY - cHeight) / mLineGradient;
            }
            else
            {
                RealX = Position.Column * tileWidth + (tileWidth / 2);
                RealY = (mLineGradient * RealX) + cHeight;
            }
        }

        public static int GetLastMaximumHeightIndex(List<GroupCoordination> coordinations)
        {
            int maxIndex = 0;
            float maximumHeight = 0;
            for (int i = 0; i < coordinations.Count; i++)
            {
                if (coordinations[i].Height >= maximumHeight)
                {
                    maxIndex = i;
                    maximumHeight = coordinations[i].Height;
                }
            }
            return maxIndex;
        }
    }

    private List<SpacePosition> ComputeLineSupercoverSet(SpacePosition source, SpacePosition target)
    {
        int y1 = source.Row;
        int x1 = source.Column;
        int y2 = target.Row;
        int x2 = target.Column;

        var points = new List<SpacePosition>();
        int dy = y2 - y1;
        int dx = x2 - x1;
        int ystep, xstep;

        int ddy = Math.Abs(dy) * 2;
        int ddx = Math.Abs(dx) * 2;

        if (dx < 0) { xstep = -1; dx = -dx; } else { xstep = 1; }
        if (dy < 0) { ystep = -1; dy = -dy; } else { ystep = 1; }

        int y = y1, x = x1;
        int error = 0;
        int errorprev;

        if (ddy > ddx)
        {
            error = dy;
            for (int i = 0; i < dy; i++)
            {
                y += ystep;
                errorprev = error;
                error += ddx;

                if (error > ddy)
                {
                    x += xstep;
                    error -= ddy;
                    if (error + errorprev < ddy)
                        points.Add(new SpacePosition(y, x - xstep));
                    else if (error + errorprev > ddy)
                        points.Add(new SpacePosition(y - ystep, x));
                    else
                    {
                        points.Add(new SpacePosition(y, x - xstep));
                        points.Add(new SpacePosition(y - ystep, x));
                    }
                }
                points.Add(new SpacePosition(y, x));
            }
        }
        else
        {
            for (int i = 0; i < dx; i++)
            {
                x += xstep;
                errorprev = error;
                error += ddy;

                if (error > ddx)
                {
                    y += ystep;
                    error -= ddx;
                    if (error + errorprev < ddx)
                        points.Add(new SpacePosition(y - ystep, x));
                    else if (error + errorprev > ddx)
                        points.Add(new SpacePosition(y, x - xstep));
                    else
                    {
                        points.Add(new SpacePosition(y - ystep, x));
                        points.Add(new SpacePosition(y, x - xstep));
                    }
                }
                points.Add(new SpacePosition(y, x));
            }
        }

        return points;
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
