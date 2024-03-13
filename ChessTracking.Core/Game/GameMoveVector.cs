namespace ChessTracking.Core.Game;

public class GameMoveVector
{
    public int X { get; set; }
    public int Y { get; set; }

    public GameMoveVector(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

}
