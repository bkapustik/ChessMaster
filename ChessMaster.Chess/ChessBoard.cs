using ChessMaster.Chess;
using ChessMaster.Space;
using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.ChessDriver;

public class ChessBoard : IChessBoard
{
    private const int boardTiles = 8;
    private readonly int padding = 2;
    private readonly int chessBoardStart;
    private readonly int chessBoardEnd;
    private SpacePosition lastFreeCaptureSpace;

    public Vector2 origin;
    private float tileWidth;

    public Space.Space Space;

    public ChessBoard()
    {
        Space = new Space.Space(boardTiles + padding*2);
        chessBoardStart = padding;
        chessBoardEnd = boardTiles + padding;
        lastFreeCaptureSpace = new SpacePosition(0, 0);
    }

    public void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = chessBoardStart; i < chessBoardEnd; i++)
        {
            Space.SubSpaces[chessBoardStart, i].Entity = new Figure(HeightProvider.GetHeight(figures[i - chessBoardStart]));
            Space.SubSpaces[chessBoardEnd - 1, i].Entity = new Figure(HeightProvider.GetHeight(figures[i - chessBoardStart]));

            Space.SubSpaces[chessBoardStart + 1, i].Entity = new Figure(HeightProvider.GetHeight(FigureType.Pawn));
            Space.SubSpaces[chessBoardEnd - 2, i].Entity = new Figure(HeightProvider.GetHeight(FigureType.Pawn));
        }
    }

    public void Initialize(Vector2 a1Center, Vector2 h8Center)
    {
        tileWidth = (float)Math.Sqrt(Math.Pow(Math.Abs(a1Center.X - h8Center.X), 2) + Math.Pow(Math.Abs(a1Center.Y - h8Center.Y), 2)) / (float)boardTiles - 1;
        origin = new Vector2(a1Center.X - tileWidth / 2, a1Center.Y - tileWidth / 2);

        for (int i = 0; i < chessBoardEnd + padding; i++)
        {
            for (int j = 0; j < chessBoardEnd + padding; j++)
            {
                Space.SubSpaces[i, j] = new SubSpace(tileWidth, new Vector2(origin.X - i * tileWidth, origin.Y - j * tileWidth));
            }
        }
        AssignFigures();
    }

    public SpacePosition GetNextFreeSpace()
    {
        var result = lastFreeCaptureSpace;

        if (lastFreeCaptureSpace.Y < chessBoardEnd + padding - 1)
        {
            lastFreeCaptureSpace.Y++;
        }
        else
        {
            lastFreeCaptureSpace = new SpacePosition(lastFreeCaptureSpace.X + 1, 0);
        }

        return result;
    }
}
