namespace ChessMaster.ChessDriver.Events;

public delegate void GameStateEvent(object? o, GameStateEventArgs e);

public class GameStateEventArgs : EventArgs
{
    public GameState GameState { get; set; }
    public GameStateEventArgs(GameState gameState)
    {
        GameState = gameState;
    }
}

public enum GameState
{ 
    InProgress,
    NotInProgress
}
