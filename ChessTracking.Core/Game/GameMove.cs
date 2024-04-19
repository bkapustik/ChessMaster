namespace ChessTracking.Core.Game;

[Serializable]
public class GameMove
{
    public ChessPosition Source { get; set; }
    public ChessPosition Target { get; set; }
    public ChessPosition? CastlingRookSource { get; set; }
    public ChessPosition? CastlingRookTarget { get; set; }
    public FigureType? CastlingFigure { get; set; }
    public FigureType SourceFigure { get; set; }
    public FigureType? TargetFigure { get; set; }

    public GameMove(GameMove gameMove)
    {
        Source = new ChessPosition(gameMove.Source);
        Target = new ChessPosition(gameMove.Target);
        SourceFigure = gameMove.SourceFigure;
        TargetFigure = gameMove.TargetFigure;
    }
    public GameMove(ChessPosition from, ChessPosition to, FigureType who, FigureType? toWhom)
    {
        Source = from;
        Target = to;
        SourceFigure = who;
        TargetFigure = toWhom;
    }

    private bool IsEquivalentWithoutCastling(GameMove other)
    {
        return
               Source.IsEquivalent(other.Source) &&
               Target.IsEquivalent(other.Target) &&
               SourceFigure == other.SourceFigure;
    }

    public bool IsEquivalent(GameMove other)
    {
        if (CastlingRookSource != null &&
            other.CastlingRookSource != null &&
            CastlingRookTarget != null &&
            other.CastlingRookTarget != null &&
            CastlingFigure != null &&
            other.CastlingFigure != null
            )
        { 
            return IsEquivalentWithoutCastling(other) &&
                CastlingRookSource.IsEquivalent(other.CastlingRookSource) &&
                CastlingRookTarget.IsEquivalent(other.CastlingRookTarget) &&
                CastlingFigure == other.CastlingFigure;
        }

        return IsEquivalentWithoutCastling(other);
    }

    public string ToUci()
    {
        if (CastlingRookSource != null && CastlingRookTarget != null && CastlingFigure == FigureType.Rook)
        {
            // Validate rook's castling positions for kingside and queenside castling
            bool isKingsideRookCorrect = CastlingRookSource.X == 7 && CastlingRookTarget.X == 5;
            bool isQueensideRookCorrect = CastlingRookSource.X == 0 && CastlingRookTarget.X == 3;

            if (Source.X == 4)
            {
                // Kingside castling
                if (Target.X == 6 && isKingsideRookCorrect)
                    return Source.Y == 0 ? "e1g1" : "e8g8";

                // Queenside castling
                if (Target.X == 2 && isQueensideRookCorrect)
                    return Source.Y == 0 ? "e1c1" : "e8c8";
            }
        }

        return $"{(char)((int)'a' + Source.X)}{Source.Y + 1}{(char)((int)'a' + Target.X)}{Target.Y + 1}";
    }
}