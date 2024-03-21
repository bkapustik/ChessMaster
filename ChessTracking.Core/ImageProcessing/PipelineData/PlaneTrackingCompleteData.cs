using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Output information of plane localization procedure
/// </summary>
public class PlaneTrackingCompleteData
{
    public KinectDataClass KinectData { get; set; }
    public TrackingResultData ResultData { get; set; }
    public PlaneTrackingData PlaneData { get; set; }
    public UserDefinedParameters UserParameters { get; set; }
    public TrackingState TrackingStateOfGame { get; set; }

    public PlaneTrackingCompleteData(InputData inputData)
    {
        KinectData = inputData.KinectData;
        UserParameters = inputData.UserParameters;
        ResultData = new TrackingResultData();
        PlaneData = new PlaneTrackingData();
        TrackingStateOfGame = inputData.TrackingStateOfGame;
    }
}
