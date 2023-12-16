namespace ChessMaster.Chess
{
    public interface IChessFigure
    {
        public ChessColor ChessColor { get; set; }
        public FigureType FigureType { get; set; }
    }
}
