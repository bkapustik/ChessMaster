namespace ChessMaster.ChessDriver.ChessMoves;

public class StartGameMove : ChessMove
{
    public StartGameMove() : base(false, "Game Started")
    { }

    public StartGameMove(string message) : base(false, message)
    { }

    public override void Execute(ChessRobot robot)
    {
        robot.StartGame();
    }
}
