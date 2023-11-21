using ChessMaster.Chess;
using ChessMaster.Space;
using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessBoard : Space.Space, IChessBoard
{
    private const int boardTiles = 8;
    public readonly SpacePosition origin;
    private readonly float tileWidth;
    private readonly IHeightProvider heightProvider;
    public SubSpace[,] Grid
    {
        get => SubSpaces;

        set
        {
            SubSpaces = Grid;
        }
    }

    public ChessBoard(int boardWidth, float physicalBoardWidth, SpacePosition origin, IHeightProvider heightProvider) : base(boardWidth)
    {
        this.origin = origin;
        this.tileWidth = (float)boardWidth / 7f;
        this.heightProvider = heightProvider;
    }

    public void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = 0; i < boardTiles; i++)
        {
            Grid[0, i].Entity = new Figure(heightProvider.GetHeight(figures[i]));
            Grid[7, i].Entity = new Figure(heightProvider.GetHeight(figures[i]));

            Grid[1, i].Entity = new Figure(heightProvider.GetHeight(FigureType.Pawn));
            Grid[6, i].Entity = new Figure(heightProvider.GetHeight(FigureType.Pawn));
        }
    }

    public void Initialize()
    {
        for (int i = 0; i < boardTiles; i++)
        {
            for (int j = 0; j < boardTiles; j++)
            {
                Grid[i, j] = new SubSpace(tileWidth, new Vector2(origin.X - i * tileWidth, origin.Y - j * tileWidth));
            }
        }
        AssignFigures();
    }
}
