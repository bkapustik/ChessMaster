namespace ChessTracking.Core.Services.Events;

public delegate void ProgramStateEvent(object? o, ProgramStateEventArgs e);

public class ProgramStateEventArgs : EventArgs
{
    public ProgramState ProgramState { get; set; }
    public ProgramStateEventArgs(ProgramState programState)
    {
        ProgramState = programState;
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