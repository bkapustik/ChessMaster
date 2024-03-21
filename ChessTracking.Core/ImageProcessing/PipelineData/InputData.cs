using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Data arriving into pipeline
/// </summary>
public class InputData
{
    public KinectDataClass KinectData;
    public TrackingResultData ResultData { get; set; }
    public UserDefinedParameters UserParameters { get; set; }
    public TrackingState TrackingStateOfGame { get; set; }

    public InputData(KinectDataClass kinectData, UserDefinedParameters userParameters, TrackingState trackingStateOfGame = null)
    {
        KinectData = kinectData;
        UserParameters = userParameters;
        ResultData = new TrackingResultData();
        TrackingStateOfGame = trackingStateOfGame;
    }
}
