using ChessMaster.Chess;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessRobot : RobotSpace
{
    private ChessBoard chessBoard;
    public ChessRobot(IRobot robot)
    {
        this.Driver = robot;
        this.chessBoard = new ChessBoard();
        this.space = chessBoard.Space;
        Events = Driver.Events;
    }

    public void InitializeChessBoard(Vector2 a1Center, Vector2 h8Center)
    {
        chessBoard.Initialize(a1Center, h8Center);
        chessBoard.AssignFigures();
    }

    public void ReconfigureChessBoard(Vector2 a1Center, Vector2 h8Center)
    {
        chessBoard.Reconfigure(a1Center, h8Center);
    }

    /// <summary>
    /// Moves figure accross classic 8x8 ChessBoard. This does not include capture position.
    /// </summary>
    /// <param name="figurePosition"></param>
    /// <param name="targetPosition"></param>
    public void MoveFigureTo(SpacePosition figurePosition, SpacePosition targetPosition)
    {
        MoveEntityFromSourceToTarget(chessBoard.GetRealSpacePosition(figurePosition),
            chessBoard.GetRealSpacePosition(targetPosition));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public void StartGame()
    {
        Driver!.ScheduleCommands(new Queue<RobotCommand>());
    }

    public void CaptureFigure(SpacePosition sourcePosition, SpacePosition targetPosition)
    {
        var freeSpace = chessBoard.GetNextFreeSpace();
        MoveEntityFromSourceToTarget(targetPosition, freeSpace);
        MoveEntityFromSourceToTarget(sourcePosition, targetPosition);
    }

    public void PromotePawn(SpacePosition source, SpacePosition target, FigureType promotion)
    {
        
    }

    public void ExecuteCastling(Castling castling)
    {
        MoveEntityFromSourceToTarget(castling.KingSource, castling.KingTarget);
        MoveEntityFromSourceToTarget(castling.RookSource, castling.RookTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException"
    /// <param name="figure"></param>
    public void ConfigurationPickUpFigure(FigureType figure)
    {
        var pickUpHeight = HeightProvider.GetHeight(figure);

        var state = GetState();
        var position = state.Position;
        position.Z = pickUpHeight;

        var commands = new Queue<RobotCommand>();
        commands.Enqueue(new OpenCommand());
        commands.Enqueue(new MoveCommand(position));
        commands.Enqueue(new CloseCommand());

        var carryPosition = state.Position;
        carryPosition.Z = HeightProvider.GetMinimalCarryingHeight();
        commands.Enqueue(new MoveCommand(carryPosition));

        Driver!.ScheduleCommands(commands);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <param name="figure"></param>
    public void ConfigurationReleaseFigure(FigureType figure)
    {
        var releaseHeight = HeightProvider.GetHeight(figure);

        var state = GetState();
        var position = state.Position;
        position.Z = releaseHeight;

        var commands = new Queue<RobotCommand>();
        commands.Enqueue(new MoveCommand(position));
        commands.Enqueue(new OpenCommand());

        var carryPosition = state.Position;
        carryPosition.Z = HeightProvider.GetMinimalCarryingHeight();
        commands.Enqueue(new CloseCommand());
        commands.Enqueue(new MoveCommand(carryPosition));

        Driver!.ScheduleCommands(commands);
    }

    public void Resume() => Driver!.Resume();

    public void Pause() => Driver!.Pause();
}