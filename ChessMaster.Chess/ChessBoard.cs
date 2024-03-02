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
    public float tileWidth { get; private set; }

    public Space.Space Space;

    public ChessBoard()
    {
        Space = new Space.Space(boardTiles + padding * 2);
        chessBoardStart = padding;
        chessBoardEnd = boardTiles + padding;
        lastFreeCaptureSpace = new SpacePosition(0, 0);
    }

    public void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = chessBoardStart; i < chessBoardEnd; i++)
        {
            Space.SubSpaces[chessBoardStart, i].Entity = ChessFigure.New(figures[i - chessBoardStart], tileWidth);
            Space.SubSpaces[chessBoardEnd - 1, i].Entity = ChessFigure.New(figures[i - chessBoardStart], tileWidth);

            Space.SubSpaces[chessBoardStart + 1, i].Entity = ChessFigure.New(FigureType.Pawn, tileWidth);
            Space.SubSpaces[chessBoardEnd - 2, i].Entity = ChessFigure.New(FigureType.Pawn, tileWidth);
        }
    }

    public void Initialize(Vector2 a1Center, Vector2 h8Center)
    {
        tileWidth = (float)Math.Abs(a1Center.X - h8Center.X) / ((float)boardTiles - 1);

        for (int i = 0; i < chessBoardEnd + padding; i++)
        {
            for (int j = 0; j < chessBoardEnd + padding; j++)
            {
                Space.SubSpaces[i, j] = new SubSpace(tileWidth, new Vector2(a1Center.X + ((i - padding) * tileWidth), a1Center.Y + ((j - padding) * tileWidth)));
            }
        }
    }

    public void Reconfigure(Vector2 a1Center, Vector2 h8Center)
    {
        tileWidth = (float)Math.Abs(a1Center.X - h8Center.X) / ((float)boardTiles - 1);

        for (int i = 0; i < chessBoardEnd + padding; i++)
        {
            for (int j = 0; j < chessBoardEnd + padding; j++)
            {
                Space.SubSpaces[i, j].Width = tileWidth;
                Space.SubSpaces[i, j].SetCenter(new Vector2(a1Center.X + ((i - padding) * tileWidth), a1Center.Y + ((j - padding) * tileWidth)));
            }
        }
    }

    public SpacePosition GetNextFreeSpace()
    {
        var result = lastFreeCaptureSpace;

        if (lastFreeCaptureSpace.Column < chessBoardEnd + padding - 1)
        {
            lastFreeCaptureSpace.Column++;
        }
        else
        {
            lastFreeCaptureSpace = new SpacePosition(lastFreeCaptureSpace.Row + 1, 0);
        }

        return result;
    }

    /// <summary>
    /// Counts in padding required for capturing figures.
    /// </summary>
    /// <param name="chessBoardPosition"></param>
    /// <returns></returns>
    public SpacePosition GetRealSpacePosition(SpacePosition chessBoardPosition) => chessBoardPosition + padding;
}
