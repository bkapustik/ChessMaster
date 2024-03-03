using ChessMaster.Chess;

namespace ChessMaster.ChessDriver.Models;

public class ChessBoardGeneral : IChessBoard
{
    public ChessFigureGeneral?[,] Grid { get; set; }
    public const int BoardDimLength = 8;
    public ChessBoardGeneral()
    {
        Grid = new ChessFigureGeneral[8, 8];
    }
    public ChessBoardGeneral(ChessFigureGeneral[,] layout)
    {
        Grid = layout;
    }
    public void Initialize()
    {
        for (int i = 0; i < BoardDimLength; i++)
        {
            for (int j = 0; j < BoardDimLength; j++)
            {
                Grid[i, j] = null;
            }
        }

        AssignFigures();
    }

    private void AssignFigures()
    {
        var figures = ((IChessBoard)this).GetFigurePlacement();

        for (int i = 0; i < figures.Count; i++)
        {
            Grid[0, i] = new ChessFigureGeneral(ChessColor.White, figures[i]);

            Grid[1, i] = new ChessFigureGeneral(ChessColor.White, FigureType.Pawn);

            Grid[BoardDimLength - 1, i] = new ChessFigureGeneral(ChessColor.Black, figures[i]);

            Grid[BoardDimLength - 2, i] = new ChessFigureGeneral(ChessColor.Black, FigureType.Pawn);
        }
    }
}

public class ChessFigureGeneral
{
    public ChessColor Color { get; set; }
    public FigureType FigureType { get; set; }
    public ChessFigureGeneral(ChessColor color)
    {
        Color = color;
    }
    public ChessFigureGeneral(ChessColor chessColor, FigureType figureType)
    {
        Color = chessColor;
        FigureType = figureType;
    }
}