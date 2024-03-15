using ChessTracking.Core.Game;
using ChessTracking.Core.Services.Events;

namespace ChessTracking.Core.Services;

public class GameController
{
    public bool IsGameValid { get; set; }
    public GameData Game { get; private set; }
    public ProgramStateEvent OnProgramStateChanged { get; set; }
    public TrackingResultProcessor TrackingProcessor { get; set; }

    public GameController()
    {
        TrackingProcessor = new TrackingResultProcessor();
    }

    public void NewGame()
    {
        Game = GameFactory.NewGame();
    }

    public void SaveGame(StreamWriter stream)
    {
        stream.Write(Game.ExportGameToAlgebraicNotation());
    }

    private void ChangeProgramState(ProgramState newState)
    {
        OnProgramStateChanged?.Invoke(this, new ProgramStateEventArgs(newState));
    }

    public void LoadGame(StreamReader stream)
    {
        var loadingResult = GameFactory.LoadGame(stream);

        if (loadingResult.LoadingSuccesfull)
        {
            Game = loadingResult.Game;

            if (Game.EndState == GameState.StillPlaying)

                ChangeProgramState(ProgramState.GameLoaded);
            else
                ChangeProgramState(ProgramState.GameFinished);
        }
    }

    public void EndGame()
    {
        Game = null;
        ChangeProgramState(ProgramState.GameEnded);
    }
}
