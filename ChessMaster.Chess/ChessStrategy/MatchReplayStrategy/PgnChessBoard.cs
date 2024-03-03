using ChessMaster.Chess;
using ChessMaster.ChessDriver.Models;

namespace ChessMaster.ChessDriver.Strategy;

public class PgnChessBoard : IChessBoard
{
    public PgnTile[,] Grid { get; set; }
    public const int BoardDimLength = 8;

    public PgnChessBoard()
    {
        Grid = new PgnTile[8, 8];
    }

    public ChessBoardGeneral ToGeneral()
    {
        var tiles = new ChessFigureGeneral[BoardDimLength, BoardDimLength];

        for (int i = 0; i < Grid.Length; i++)
        {
            for (int j = 0; j < BoardDimLength; j++)
            {
                if (Grid[i, j].Figure != null)
                {
                    tiles[i, j].FigureType = Grid[i, j]!.Figure!.FigureType;
                    tiles[i, j].Color = Grid[i, j]!.Figure!.ChessColor;
                }
            }
        }

        return new ChessBoardGeneral(tiles);
    }

    public void Initialize()
    {
        for (int i = 0; i < BoardDimLength; i++)
        {
            for (int j = 0; j < BoardDimLength; j++)
            {
                Grid[i, j] = new PgnTile();
                if (i % 2 == 0)
                {
                    if (j % 2 == 0)
                    {
                        Grid[i, j].ChessColor = ChessColor.Black;
                    }
                }
                else
                {
                    if (j % 2 == 0)
                    {
                        Grid[i, j].ChessColor = ChessColor.White;
                    }
                }
            }
        }

        AssignFigures();
    }

    private void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = 0; i < figures.Count; i++)
        {
            Grid[0, i].Figure = PgnFigure.CreateFigure(ChessColor.White, figures[i], Grid);

            Grid[1, i].Figure = PgnFigure.CreateFigure(ChessColor.White, FigureType.Pawn, Grid);

            Grid[BoardDimLength - 1, i].Figure = PgnFigure.CreateFigure(ChessColor.Black, figures[i], Grid);

            Grid[BoardDimLength - 2, i].Figure = PgnFigure.CreateFigure(ChessColor.Black, FigureType.Pawn, Grid);
        }
    }
}