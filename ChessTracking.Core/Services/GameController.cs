using ChessTracking.Core.Game;
using ChessTracking.Core.Services.Events;
using ChessTracking.Core.Tracking.State;
using ChessTracking.Core.Utils;

namespace ChessTracking.Core.Services
{
    public class GameController
    {
        public bool IsGameValid { get; set; }
        public GameData Game { get; private set; }

        public ProgramStateEvent OnProgramStateChanged { get; set; }

        public GameController()
        {
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

        public TrackingState GetTrackingState()
        {
            return Game?.Chessboard?.GetTrackingStates();
        }

        public int? InitiateWithTracingInput(TrackingState trackingState)
        {
            var figures = Game.Chessboard.GetTrackingStates().Figures;

            var chessboardState = new TrackingState(figures);

            for (int i = 0; i < 4; i++)
            {
                if (TrackingState.IsEquivalent(chessboardState, trackingState))
                {
                    IsGameValid = true;
                    return i;
                }
                trackingState.RotateClockWise(1);
            }

            return null;
        }

        public void TryChangeChessboardState(TrackingState trackingState)
        {
            if (Game.EndState == GameState.StillPlaying)
            {
                var validationResult = GameValidator.ValidateAndPerform(Game.DeepClone(), trackingState); // get from validator

                if (validationResult.IsValid)
                    Game = validationResult.NewGameState;

                if (trackingState.IsEquivalentTo(Game.Chessboard.GetTrackingStates()))
                    IsGameValid = true;
                else
                    IsGameValid = validationResult.IsValid;

                if (Game.EndState != GameState.StillPlaying)
                {
                    ChangeProgramState(ProgramState.GameFinished);
                }
            }
        }
    }
}
