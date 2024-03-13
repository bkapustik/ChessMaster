using ChessTracking.Core.Game;
using ChessTracking.Core.ImageProcessing.PipelineParts.Events;
using ChessTracking.Core.Services.Events;
using ChessTracking.Core.Tracking;
using ChessTracking.Core.Tracking.State;
using ChessTracking.Core.Utils;
using System.Drawing;

namespace ChessTracking.Core.Services;

public class TrackingResultProcessor
{
    private GameController GameController { get; }
    private FPSCounter FpsCounter { get; }
    public GameData Game { get; set; }

    /// <summary>
    /// Queue containing latest tracking states, so they can be averaged
    /// </summary>
    private Queue<TimestampObject<TrackingState>> AveragingQueue { get; set; }

    /// <summary>
    /// Last state sent to game component
    /// </summary>
    private TrackingState LastSentState { get; set; }

    private bool TrackingInProgress { get; set; }

    /// <summary>
    /// Offset of state processing after reset
    /// </summary>
    private DateTime TimeOffset { get; set; }

    /// <summary>
    /// Defines number of right rotation between tracking state and game representation
    /// </summary>
    public int NumberOfCwRotations { get; private set; }

    /// <summary>
    /// Bitmap of chessboard for displaying chessboard tracking state
    /// </summary>
    private static Bitmap ChessboardBitmap { get; set; }

    public SceneCalibrationSnapshotEvent? OnSceneCalibrationSnapshotChanged { get; set; }
    public HandDetectedEvent? OnHandDetectionEvent { get; set; }
    public VizualizationUpdatedEvent? OnVizualizationUpdadted { get; set; }
    public BoardUpdatedEvent? OnImmediateBoardUpdated { get; set; }
    public AveragedBoardUpdatedEvent? OnAveragedBoardUpdated { get; set; }
    public GameRecognizedEvent? OnGameRecognized { get; set; }
    public ErrorOccuredEvent? OnErrorOccured { get; set; }
    public GameStartedEvent? OnGameStarted { get; set; }
    public FpsUpdatedEvent? OnFpsUpdated { get; set; }

    public TrackingResultProcessor(GameData game)
    {
        FpsCounter = new FPSCounter();
        AveragingQueue = new Queue<TimestampObject<TrackingState>>();
        TimeOffset = DateTime.Now + TimeSpan.FromSeconds(1.5);
        Game = game;
        TrackingInProgress = false;
    }

    public void Reset()
    {
        TrackingInProgress = false;
        NumberOfCwRotations = 0;
        LastSentState = null;
        AveragingQueue.Clear();
        TimeOffset = DateTime.Now + TimeSpan.FromSeconds(1.5);
    }

    public void ProcessResult(TrackingResult result)
    {
        HandDetected();
        UpdateVizualization(result.BitmapToDisplay);
        UpdateFps();

        if (result.TrackingState == null)
            return;
        if (result.HandDetected)
            return;
        if (TimeOffset > DateTime.Now)
            return;

        var trackingState = result.TrackingState;
        trackingState.HorizontalFlip();

        var pointCounts = result.PointCountsOverFields;
        pointCounts = pointCounts.FlipHorizontally();

        if (TrackingInProgress)
        {
            trackingState.RotateClockWise(NumberOfCwRotations);
            pointCounts = pointCounts.RotateArray90DegCcwNTimes(NumberOfCwRotations);

            UpdateImmediateBoard(GenerateImageForTrackingState(trackingState, pointCounts));
            var average = PerformAveraging(trackingState);
            if (average == null)
                return;

            // send averaging
            UpdateAveragedBoard(GenerateImageForTrackingState(average, null, GameController.GetTrackingState()));
            // check so we aren't sending the same state again
            if (LastSentState != null && !LastSentState.IsEquivalentTo(average))
            {
                // send it to the game
                GameController.TryChangeChessboardState(average);
            }

            LastSentState = average;
        }
        else
        {
            UpdateImmediateBoard(GenerateImageForTrackingState(trackingState, pointCounts));
            // averaging
            var average = PerformAveraging(trackingState);
            if (average == null)
                return;
            else
            {
                UpdateAveragedBoard(GenerateImageForTrackingState(average, null));
                var rotation = GameController.InitiateWithTracingInput(average);
                if (rotation.HasValue)
                {
                    RaiseGameRecognized();
                    NumberOfCwRotations = rotation.Value;
                    RotateSavedStates();
                    TrackingInProgress = true;
                }
            }
        }
    }

    /// <summary>
    /// Render tracking state image for displaying
    /// </summary>
    private Bitmap GenerateImageForTrackingState(TrackingState trackingState, int[,] pointCounts, TrackingState gameTrackingState = null)
    {
        trackingState = new TrackingState(trackingState.Figures);

        var bm = (Bitmap)ChessboardBitmap.Clone();
        SolidBrush blackBrush = new SolidBrush(Color.Black);
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush redBrush = new SolidBrush(Color.Red);

        Font font = new Font(FontFamily.GenericSerif, 4, FontStyle.Bold);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                using (Graphics graphics = Graphics.FromImage(bm))
                {
                    // invertion of y coordinate due to differences between chess and bitmap coordinates
                    switch (trackingState.Figures[x, 7 - y])
                    {
                        case TrackingFieldState.White:
                            graphics.FillRectangle(whiteBrush, new Rectangle(x * 40, y * 40, 40, 40));
                            break;
                        case TrackingFieldState.Black:
                            graphics.FillRectangle(blackBrush, new Rectangle(x * 40, y * 40, 40, 40));
                            break;
                        case TrackingFieldState.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (pointCounts != null)
                        graphics.DrawString(pointCounts[x, 7 - y].ToString(), font, redBrush, x * 40, y * 40);

                    if (gameTrackingState != null && gameTrackingState.Figures[x, 7 - y] != trackingState.Figures[x, 7 - y])
                    {
                        graphics.FillRectangle(redBrush, new Rectangle(x * 40 + 12, y * 40 + 12, 16, 16));
                    }
                }
            }
        }

        return bm;
    }

    /// <summary>
    /// Rotate currently saved states in averaging queue
    /// </summary>
    private void RotateSavedStates()
    {
        foreach (var averageState in AveragingQueue)
        {
            averageState.StoredObject.RotateClockWise(NumberOfCwRotations);
        }
    }

    /// <summary>
    /// Performs averaging of currently saved states
    /// </summary>
    /// <param name="trackingState">Arrived state</param>
    /// <returns>Averaged result</returns>
    private TrackingState PerformAveraging(TrackingState trackingState)
    {
        AveragingQueue.Enqueue(new TimestampObject<TrackingState>(trackingState));

        var now = DateTime.Now;

        // discard all states older than x seconds
        var temp = AveragingQueue.ToList();
        temp.RemoveAll(x => Math.Abs((now - x.Timestamp).Seconds) > 2);

        AveragingQueue = new Queue<TimestampObject<TrackingState>>(temp);

        // don't average if there aren't enough samples
        if (AveragingQueue.Count <= 3)
            return null;

        // choose most common tracking state
        List<Tuple<TrackingState, int>> aggregation = new List<Tuple<TrackingState, int>>();
        foreach (var state in AveragingQueue)
        {
            if (aggregation.Any(x => x.Item1.IsEquivalentTo(state.StoredObject)))
            {
                var old = aggregation.Single(x => x.Item1.IsEquivalentTo(state.StoredObject));
                aggregation.Remove(old);
                aggregation.Add(new Tuple<TrackingState, int>(old.Item1, old.Item2 + 1));
            }
            else
            {
                aggregation.Add(new Tuple<TrackingState, int>(state.StoredObject, 1));
            }
        }

        return aggregation.OrderByDescending(x => x.Item2).First().Item1;
    }

    public void RaiseError(string message)
    {
        OnErrorOccured?.Invoke(this, new ErrorOccuredEventArgs(message));
    }
    public void TrackingStarted()
    {
        OnGameStarted?.Invoke(this, new GameStartedEventArgs());
    }
    public void SceneCalibrationSnapshotChanged(SceneCalibrationSnapshot snapshot)
    {
        OnSceneCalibrationSnapshotChanged?.Invoke(this, new SceneCalibrationSnapshotEventArgs(snapshot));
    }

    private void UpdateFps()
    {
        int? fps = FpsCounter.Update();
        if (fps != null)
        {
            OnFpsUpdated?.Invoke(this, new FpsUpdatedEventArgs(fps.Value));
        }
    }
    private void RaiseGameRecognized()
    {
        OnGameRecognized?.Invoke(this, new GameRecognizedEventArgs());
    }
    private void HandDetected()
    {
        OnHandDetectionEvent?.Invoke(this, new HandDetectionEventArgs());
    }
    private void UpdateVizualization(Bitmap bitmap)
    {
        OnVizualizationUpdadted?.Invoke(this, new VizualizationUpdateEventArgs(bitmap));
    }
    private void UpdateImmediateBoard(Bitmap bitmap)
    {
        OnImmediateBoardUpdated?.Invoke(this, new BoardUpdatedEventArgs(bitmap));
    }
    private void UpdateAveragedBoard(Bitmap bitmap)
    {
        OnAveragedBoardUpdated?.Invoke(this, new BoardUpdatedEventArgs(bitmap));
    }
}