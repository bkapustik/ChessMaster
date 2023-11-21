using ChessMaster.Chess;

namespace ChessMaster.Chess;

public interface IChessBoard
{
    void Initialize();
    List<FigureType> GetFigurePlacement() =>
        new List<FigureType>()
        {
            FigureType.Rook,
            FigureType.Knight,
            FigureType.Bishop,
            FigureType.Queen,
            FigureType.King,
            FigureType.Bishop,
            FigureType.Knight,
            FigureType.Rook
        };
}