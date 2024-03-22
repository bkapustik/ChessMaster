using ChessTracking.Core.Game;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services.Events;

namespace ChessTracking.Core.Services;

public class GameController
{
    public bool IsGameValid { get; set; }
    public GameData Game { get; private set; }
    public ProgramStateEvent? OnProgramStateChanged { get; set; }
    public TrackingResultProcessor TrackingProcessor { get; set; }
    public TrackingController TrackingController { get; set; }

    public GameController(UserDefinedParametersPrototypeFactory parameters)
    {
        TrackingProcessor = new TrackingResultProcessor();
        TrackingController = new TrackingController(parameters, TrackingProcessor);
    }

    public void NewGame()
    {
        Game = GameFactory.NewGame();
        TrackingProcessor.InitializeGame(Game);
    }

    public void SaveGame(StreamWriter stream)
    {
        stream.Write(Game.ExportGameToAlgebraicNotation());
    }

    private void ChangeProgramState(ProgramState newState)
    {
        OnProgramStateChanged?.Invoke(this, new ProgramStateEventArgs(newState));
    }

    public bool TryLoadGame(StreamReader stream)
    {
        var loadingResult = GameFactory.LoadGame(stream);

        if (loadingResult.LoadingSuccesfull)
        {
            Game = loadingResult.Game;

            if (Game.EndState == GameState.StillPlaying)

                ChangeProgramState(ProgramState.GameLoaded);
            else
                ChangeProgramState(ProgramState.GameFinished);

            TrackingProcessor.InitializeGame(Game);

            return true;
        }

        return false;
    }

    public void EndGame()
    {
        Game = null;
        ChangeProgramState(ProgramState.GameEnded);
    }
}
