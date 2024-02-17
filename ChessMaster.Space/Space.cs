using ChessMaster.Space.Coordinations;

namespace ChessMaster.Space;

public class Space : Entity
{
    public SubSpace[,] SubSpaces { get; set; }
    public SpacePosition Origin { get; set; }

    public Space(int numberOfTiles) 
    {
        this.SubSpaces = new SubSpace[numberOfTiles, numberOfTiles];
    }

    public Space(int width, int length)
    {
        this.SubSpaces = new SubSpace[length, width];
    }
}
