using ChessMaster.Chess;
using ChessMaster.Space;
using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessBoard : Space.Space, IChessBoard
{
    private const int boardTiles = 8;
    public Vector2 origin;
    private float tileWidth;

    public SubSpace[,] Grid
    {
        get => SubSpaces;

        set
        {
            SubSpaces = Grid;
        }
    }

    public ChessBoard() : base(boardTiles)
    {
    }

    public void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = 0; i < boardTiles; i++)
        {
            Grid[0, i].Entity = new Figure(HeightProvider.GetHeight(figures[i]));
            Grid[7, i].Entity = new Figure(HeightProvider.GetHeight(figures[i]));

            Grid[1, i].Entity = new Figure(HeightProvider.GetHeight(FigureType.Pawn));
            Grid[6, i].Entity = new Figure(HeightProvider.GetHeight(FigureType.Pawn));
        }
    }

    public void Initialize(Vector2 a1Center, Vector2 h8Center)
    {
        tileWidth = (float)Math.Sqrt(Math.Pow(Math.Abs(a1Center.X - h8Center.X), 2) + Math.Pow(Math.Abs(a1Center.Y - h8Center.Y), 2)) / (float)boardTiles - 1;
        origin = new Vector2(a1Center.X - tileWidth/2, a1Center.Y - tileWidth / 2);
        Width = tileWidth * 8;
        Length = tileWidth * 8;

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
