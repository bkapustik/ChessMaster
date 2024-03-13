using ChessTracking.Core.ImageProcessing.PipelineData;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.StagesInterfaces;

/// <summary>
/// Wraps up whole functionality that detects plane with chessboard
/// </summary>
interface IPlaneLocalization
{
    /// <summary>
    /// Method called while calibrating to the given scene
    /// </summary>
    /// <param name="inputData">Data from kinect sensor</param>
    /// <returns>Data with calibrated plane</returns>
    PlaneTrackingCompleteData Calibrate(InputData inputData);

    /// <summary>
    /// Method called while tracking given scene
    /// </summary>
    /// <param name="inputData">Data from kinect sensor</param>
    /// <returns>Data with tracked plane</returns>
    PlaneTrackingCompleteData Track(InputData inputData);
}
