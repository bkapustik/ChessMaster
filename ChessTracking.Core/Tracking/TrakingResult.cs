using ChessTracking.Core.Tracking.State;
using System.Drawing;

namespace ChessTracking.Core.Tracking;

public class TrackingResult
{
    public Bitmap BitmapToDisplay { get; }
    public TrackingState TrackingState { get; }
    public bool HandDetected { get; }
    public int[,] PointCountsOverFields { get; }

    public TrackingResult(Bitmap bitmapToDisplay, TrackingState trackingState, bool handDetected, int[,] pointCountsOverFields)
    {
        BitmapToDisplay = bitmapToDisplay;
        TrackingState = trackingState;
        HandDetected = handDetected;
        PointCountsOverFields = pointCountsOverFields;
    }
}
