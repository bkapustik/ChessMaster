using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.ImageProcessing.PipelineParts.StagesInterfaces;
using ChessTracking.Core.ImageProcessing.FigureAlgorithms;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.Stages;

public class FiguresLocalization : IFiguresLocalization
{
    private HandDetectionAlgorithm HandDetectionAlgorithm { get; }
    private FiguresLocalizationAlgorithm FiguresLocalizationAlgorithm { get; }

    public FiguresLocalization()
    {
        HandDetectionAlgorithm = new HandDetectionAlgorithm();
        FiguresLocalizationAlgorithm = new FiguresLocalizationAlgorithm();
    }

    public FiguresTrackingCompleteData Calibrate(ChessboardTrackingCompleteData chessboardData)
    {
        var figuresData = new FiguresTrackingCompleteData(chessboardData);

        return figuresData;
    }

    public FiguresTrackingCompleteData Track(ChessboardTrackingCompleteData chessboardData)
    {
        var figuresData = new FiguresTrackingCompleteData(chessboardData);

        (figuresData.ResultData.TrackingState, figuresData.ResultData.PointCountsOverFields) =
            FiguresLocalizationAlgorithm.LocateFigures(
                figuresData.KinectData,
                figuresData.ChessboardData.FieldSize,
                figuresData.PlaneData.CannyDepthData,
                figuresData.UserParameters,
                figuresData.ResultData,
                figuresData.PlaneData.ColorBitmap,
                chessboardData.TrackingStateOfGame);

        var handDetected =
            HandDetectionAlgorithm.HandDetected(
                figuresData.KinectData.CameraSpacePointsFromDepthData,
                figuresData.ChessboardData.FieldSize,
                figuresData.UserParameters
            );
        figuresData.ResultData.SceneDisrupted = handDetected || figuresData.ResultData.SceneDisrupted;

        return figuresData;
    }
}
