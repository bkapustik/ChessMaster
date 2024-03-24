using ChessTracking.Core.Game;
using ChessTracking.Core.Services.Events;

namespace ChessTracking.Core.Utils;

public static class EnumEtensions
{
    public static string GetString(this GameState gameState)
    { 
        switch (gameState) 
        {
            case GameState.WhiteWin:
                return nameof(GameState.WhiteWin);
            case GameState.BlackWin:
                return nameof(GameState.BlackWin);
            case GameState.Draw:
                return nameof(GameState.Draw);
            default:
                return "";
        }
    }

    public static string GetString(this ProgramState state)
    { 
        switch(state) 
        {
            case ProgramState.GameLoaded:
                return nameof(ProgramState.GameLoaded);
            case ProgramState.GameFinished:
                return nameof(ProgramState.GameFinished);
            case ProgramState.GameEnded:
                return nameof(ProgramState.GameEnded);
            case ProgramState.StartedTracking:
                return nameof(ProgramState.StartedTracking);
            case ProgramState.StoppedTracking:
                return nameof(ProgramState.StoppedTracking);
            case ProgramState.Recalibrating:
                return nameof(ProgramState.Recalibrating);
            case ProgramState.ErrorInTracking:
                return nameof(ProgramState.ErrorInTracking);
            case ProgramState.GameRecognized:
                return nameof(ProgramState.GameRecognized);
            default:
                return "";
        }
    }
}
