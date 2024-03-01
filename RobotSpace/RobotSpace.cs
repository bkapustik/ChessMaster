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

        TakeEntityFromPosition(source);

        //MoveToCarryingPosition(target);

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
        var commands = new Queue<RobotCommand>();

        space!.SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;
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

        Driver!.ScheduleCommands(commands);
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

        SpacePosition currentPosition = new SpacePosition((int)(expectedResultingPosition.X / TileWidth), (int)(expectedResultingPosition.Y / TileWidth));

        List<SpacePosition> intersectedTiles = ComputeLineSupercoverSet(currentPosition, targetPosition);

        if (intersectedTiles.Count > 0)
        {
            float sourcePointX = expectedResultingPosition.X;
            float sourcePointY = expectedResultingPosition.Y;
            float targetPointX = (targetPosition.X * TileWidth) + (TileWidth / 2);
            float targetPointY = (targetPosition.Y * TileWidth) + (TileWidth / 2);

            float mLineGradient = (targetPointY - sourcePointY) / (targetPointX - sourcePointX);
            float cHeight = -(mLineGradient * sourcePointX) + sourcePointY;

            int dx = Math.Abs(targetPosition.X - currentPosition.X);
            int dy = Math.Abs(targetPosition.Y - currentPosition.Y);

            IEnumerable<IGrouping<int, SpacePosition>> tilesGrouped;

            if (dx > dy)
            {
                tilesGrouped = intersectedTiles.GroupBy(tile => tile.Y);
            }
            else
            {
                tilesGrouped = intersectedTiles.GroupBy(tile => tile.X);
            }

            List<GroupCoordination> groupCoordinations = tilesGrouped.Select(group => group
                .MaxBy(tile => space!.SubSpaces[tile.X, tile.Y].Entity?.Height ?? 0))
                .Select(tile => new GroupCoordination(tile,
                    space!.SubSpaces[tile.X, tile.Y].Entity?.Height ?? 0)
                ).ToList();

            foreach (var group in groupCoordinations)
            {
                group.ComputeTileIntersectionTowardsTarget(dx, dy, sourcePointX, sourcePointY, TileWidth, cHeight, mLineGradient);
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

        public void ComputeTileIntersectionTowardsTarget(int dx, int dy, float sourcePointX, float sourcePointY, float tileWidth, float cHeight, float mLineGradient)
        {
            if (dx > dy)
            {
                RealX = Position.X * tileWidth + (tileWidth / 2);
                RealY = (mLineGradient * (RealX - sourcePointX)) + sourcePointY;
            }
            else
            {
                RealY = Position.Y * tileWidth + (tileWidth / 2);
                RealX = ((RealY - sourcePointY) / mLineGradient) + sourcePointX;
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
        int x1 = source.X;
        int y1 = source.Y;
        int x2 = target.X;
        int y2 = target.Y;

        var points = new List<SpacePosition>();
        int dx = x2 - x1;
        int dy = y2 - y1;
        int xstep, ystep;

        int ddx = Math.Abs(dx) * 2;
        int ddy = Math.Abs(dy) * 2;

        if (dy < 0) { ystep = -1; dy = -dy; } else { ystep = 1; }
        if (dx < 0) { xstep = -1; dx = -dx; } else { xstep = 1; }

        int x = x1, y = y1;
        int error = ddx - dy;
        int errorprev;

        if (ddx >= ddy)
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
                        points.Add(new SpacePosition(x, y - ystep));
                    else if (error + errorprev > ddx)
                        points.Add(new SpacePosition(x - xstep, y));
                    else
                    {
                        points.Add(new SpacePosition(x, y - ystep));
                        points.Add(new SpacePosition(x - xstep, y));
                    }
                }
                points.Add(new SpacePosition(x, y));
            }
        }
        else
        {
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
                        points.Add(new SpacePosition(x - xstep, y));
                    else if (error + errorprev > ddy)
                        points.Add(new SpacePosition(x, y - ystep));
                    else
                    {
                        points.Add(new SpacePosition(x - xstep, y));
                        points.Add(new SpacePosition(x, y - ystep));
                    }
                }
                points.Add(new SpacePosition(x, y));
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
