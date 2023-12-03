using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess.Strategy.MatchReplay;

public class PgnChessBoard : IChessBoard
{
    public PgnTile[,] Grid { get; set; }
    public const int BoardDimLength = 8;

    public PgnChessBoard()
    {
        Grid = new PgnTile[8, 8];
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