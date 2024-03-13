using ChessTracking.Core.ImageProcessing.PipelineData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.StagesInterfaces;

/// <summary>
/// Wraps up whole functionality that detects objects on and over chessboard
/// </summary>
public interface IFiguresLocalization
{
    /// <summary>
    /// Method called while calibrating to the given scene
    /// </summary>
    /// <param name="chessboardData">Data from previous stages of calibration, especially chessboard calibration</param>
    /// <returns>Data with calibration informations about figures</returns>
    FiguresTrackingCompleteData Calibrate(ChessboardTrackingCompleteData chessboardData);

    /// <summary>
    /// Method called while tracking given scene
    /// </summary>
    /// <param name="chessboardData">Data from previous stages of tracking, especially chessboard tracking</param>
    /// <returns>Data with tracked figures</returns>
    FiguresTrackingCompleteData Track(ChessboardTrackingCompleteData chessboardData);
}
