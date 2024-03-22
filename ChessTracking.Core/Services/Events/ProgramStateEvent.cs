using ChessTracking.Core.Game;

namespace ChessTracking.Core.Services.Events;

public delegate void ProgramStateEvent(object? o, ProgramStateEventArgs e);

public class ProgramStateEventArgs : EventArgs
{
    public ProgramState ProgramState { get; set; }
    public GameState? GameState { get; set; }
    public ProgramStateEventArgs(ProgramState programState, GameState? gameState = null)
    {
        ProgramState = programState;
        GameState = gameState;
    }
}

public enum ProgramState
{ 
    GameLoaded,
    GameFinished,
    GameEnded,
    StartedTracking,
    StoppedTracking,
    Recalibrating,
    ErrorInTracking,
    GameRecognized
}