namespace ChessTracking.Core.Game;

[Serializable]
public class GameMove
{
    public ChessPosition From { get; set; }
    public ChessPosition To { get; set; }
    public FigureType Who { get; set; }
    public FigureType? ToWhom { get; set; }

    public GameMove(GameMove gameMove)
    {
        From = new ChessPosition(gameMove.From);
        To = new ChessPosition(gameMove.To);
        Who = gameMove.Who;
        ToWhom = gameMove.ToWhom;
    }
    public GameMove(ChessPosition from, ChessPosition to, FigureType who, FigureType? toWhom)
    {
        From = from;
        To = to;
        Who = who;
        ToWhom = toWhom;
    }

    public bool IsEquivalent(GameMove other)
    {
        return
            From.IsEquivalent(other.From) &&
            To.IsEquivalent(other.To) &&
            Who == other.Who;
    }

    public string ToUci()
    {
        return $"{(char)((int)'a' + From.X)}{From.Y + 1}{(char)((int)'a' + To.X)}{To.Y + 1}";
    }
}