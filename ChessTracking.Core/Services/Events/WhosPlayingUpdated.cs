using ChessTracking.Core.Game;

namespace ChessTracking.Core.Services.Events;

public delegate void WhosPlayingUpdated(object o, WhosPlayingUpdatedEventArgs e);

public class WhosPlayingUpdatedEventArgs : EventArgs
{
    public PlayerColor PlayerColor { get; set; }
    public WhosPlayingUpdatedEventArgs(PlayerColor playerColor)
    {
        PlayerColor = playerColor;
    }
}