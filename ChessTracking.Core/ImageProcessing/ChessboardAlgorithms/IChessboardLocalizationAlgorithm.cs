using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms;

interface IChessboardLocalizationAlgorithm
{
    (Chessboard3DReprezentation boardReprezentation, SceneCalibrationSnapshot snapshot) LocateChessboard(ChessboardTrackingCompleteData chessboardData);
}
