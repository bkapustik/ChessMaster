using ChessMaster.Chess;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using ChessMaster.Space.RobotSpace;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessRobot : RobotSpace
{
    private ChessBoard chessBoard;

    public ChessRobot(string portName)
    {
        this.Robot = new Robot(portName);
        this.chessBoard = new ChessBoard();
        this.space = chessBoard.Space;
    }
    public ChessRobot(IRobot robot)
    {
        this.Robot = robot;
        this.chessBoard = new ChessBoard();
        this.space = chessBoard.Space;
    }
    public void InitializeChessBoard(Vector2 a1Center, Vector2 h8Center)
    {
        chessBoard.Initialize(a1Center, h8Center);
    }
    public void MoveFigureTo(SpacePosition figurePosition, SpacePosition targetPosition)
    {
        MoveEntityFromSourceToTarget(figurePosition, targetPosition);
    }
    public void StartGame()
    {
        Robot.ScheduleCommands(new Queue<RobotCommand>());
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

    public void ConfigurationPickPawn()
    {
        //robot.OpenGrip();
        
        //configurationHeight = robot.GetState().Result.Position.Z;

        //robot.MoveZ(HeightProvider.GetHeight(FigureType.Pawn));

        //robot.CloseGrip();
    }

    public void ConfigurationReleasePawn()
    {
        //robot.OpenGrip();

        //robot.CloseGrip();

        //robot.MoveZ(configurationHeight);
    }
}