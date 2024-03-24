using ChessMaster.ChessDriver.Events;
using ChessTracking.Core.Game;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;
using ChessTracking.Core.Services.Events;

namespace ChessMaster.ChessDriver.Services;

public class KinectService : IKinectService
{
    public GameController GameController { get; private set; }
    public UserDefinedParametersPrototypeFactory UserDefinedParameters { get; private set; }
    public KinectMoveDetectedEvent? OnKinectMoveDetected { get; set; }
    private List<string> MoveMessages { get; set; } = new List<string>();
    private List<GameMove> GameMoves { get; set; } = new List<GameMove>();

    public KinectService()
    {
        UserDefinedParameters = new UserDefinedParametersPrototypeFactory();
        GameController = new GameController(UserDefinedParameters);

        GameController.TrackingProcessor.OnRecordStateUpdated += ReactToMovesUpdate;
    }

    public void Dispose()
    {
        GameController.TrackingProcessor.OnRecordStateUpdated -= ReactToMovesUpdate;
    }

    private void ReactToMovesUpdate(object o, RecordStateUpdatedEventArgs e)
    {
        if (!e.GameMoves.Any())
        {
            return;
        }

        var lastMove = e.GameMoves.Last();
        var lastMoveString = e.GameMoves.Last().ToUci();

        if (GameMoves.Any())
        {
            if (lastMove.IsEquivalent(GameMoves.Last()))
            {
                return;
            }
            else
            {
                GameMoves.Add(lastMove);
                MoveMessages.Add(lastMoveString);
            }
        }
        else
        {
            GameMoves.Add(lastMove);
            MoveMessages.Add(lastMoveString);
        }

        OnKinectMoveDetected?.Invoke(this, new KinectMoveDetectedEventArgs(lastMoveString, lastMove));
    }
}
