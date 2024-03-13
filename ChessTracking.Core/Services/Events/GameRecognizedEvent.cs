namespace ChessTracking.Core.Services.Events;

public delegate void GameRecognizedEvent(object? o, GameRecognizedEventArgs e);

public class GameRecognizedEventArgs
{
    public GameRecognizedEventArgs()
    {
        
    }
}