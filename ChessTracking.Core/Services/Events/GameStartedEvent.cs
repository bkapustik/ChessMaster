namespace ChessTracking.Core.Services.Events;

public delegate void GameStartedEvent(object? o, GameStartedEventArgs e);

public class GameStartedEventArgs : EventArgs
{
    public GameStartedEventArgs()
    {
        
    }
}
