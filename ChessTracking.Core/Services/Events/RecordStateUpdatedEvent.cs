using ChessTracking.Core.Game;

namespace ChessTracking.Core.Services.Events;

public delegate void RecordStateUpdatedEvent(object o, RecordStateUpdatedEventArgs e);

public class RecordStateUpdatedEventArgs : EventArgs
{
    public IList<GameMove> GameMoves { get; set; }
    public List<string> RecordOfGame { get; set; }
    public RecordStateUpdatedEventArgs(IList<GameMove> gameMoves, List<string> recordOfGame)
    {
        GameMoves = gameMoves;
        RecordOfGame = recordOfGame;
    }
}