using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Output information of figures localization procedure
/// </summary>
public class FiguresTrackingCompleteData
{
    public KinectDataClass KinectData { get; set; }
    public TrackingResultData ResultData { get; set; }
    public PlaneTrackingData PlaneData { get; set; }
    public ChessboardTrackingData ChessboardData { get; set; }
    public UserDefinedParameters UserParameters { get; set; }
    public TrackingState TrackingStateOfGame { get; set; }

    public FiguresTrackingCompleteData(ChessboardTrackingCompleteData chessboardData)
    {
        KinectData = chessboardData.KinectData;
        ResultData = chessboardData.ResultData;
        PlaneData = chessboardData.PlaneData;
        ChessboardData = chessboardData.ChessboardData;
        UserParameters = chessboardData.UserParameters;
        TrackingStateOfGame = chessboardData.TrackingStateOfGame;
    }
}
