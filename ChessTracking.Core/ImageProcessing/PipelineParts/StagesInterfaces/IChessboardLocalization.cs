using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.ImageProcessing.PipelineParts.Events;
using ChessTracking.Core.Tracking.State;
using System.Collections.Concurrent;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.StagesInterfaces;

/// <summary>
/// Wraps up whole functionality that looks for position of chessboard
/// </summary>
interface IChessboardLocalization
{
    /// <summary>
    /// Method called while calibrating to the given scene
    /// </summary>
    /// <param name="planeData">Data from previous stages of calibration, especially plane calibration</param>
    /// <returns>Data with calibrated chessboard position</returns>
    ChessboardTrackingCompleteData Calibrate(PlaneTrackingCompleteData planeData, out SceneCalibrationSnapshot snapshot);

    /// <summary>
    /// Method called while tracking given scene
    /// </summary>
    /// <param name="planeData">Data from previous stages of tracking, especially plane tracking</param>
    /// <returns>Data with tracked chessboard</returns>
    ChessboardTrackingCompleteData Track(PlaneTrackingCompleteData planeData);

    /// <summary>
    /// Moves model chessboard by one edge vector of one chessboard field
    /// </summary>
    /// <param name="direction">direction in which to move</param>
    void MoveChessboard(ChessboardMovement direction);
}
