using ChessTracking.Core.Tracking.State;
using System.Drawing;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Results of tracking
/// </summary>
public class TrackingResultData
{
    public TrackingState TrackingState { get; set; }
    public bool SceneDisrupted { get; set; }
    public Bitmap VisualisationBitmap { get; set; }
    public int[,] PointCountsOverFields { get; set; }
}
