using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Output information of chessboard localization procedure
/// </summary>
public class ChessboardTrackingCompleteData
{
    public KinectDataClass KinectData { get; set; }
    public TrackingResultData ResultData { get; set; }
    public PlaneTrackingData PlaneData { get; set; }
    public ChessboardTrackingData ChessboardData { get; set; }
    public UserDefinedParameters UserParameters { get; set; }
    public TrackingState TrackingStateOfGame { get; set; }

    public ChessboardTrackingCompleteData(PlaneTrackingCompleteData planeData)
    {
        KinectData = planeData.KinectData;
        ResultData = planeData.ResultData;
        PlaneData = planeData.PlaneData;
        UserParameters = planeData.UserParameters;
        ChessboardData = new ChessboardTrackingData();
        TrackingStateOfGame = planeData.TrackingStateOfGame;
    }
}
