using ChessMaster.Space;

namespace ChessMaster.ChessDriver.Board;

public class ChessBoard : Space.Space
{
    public SubSpace[,] Grid 
    { 
        get => SubSpaces;
        
        set 
        {
            SubSpaces = Grid;
        }
    }

    public ChessBoard(float width, float height) : base(width, height)
    {
        SubSpaces = new Space.SubSpace[8, 8];
        AssignFigures();
    }

    private void AssignFigures()
    {
        throw new NotImplementedException();
    }
}
