﻿using ChessTracking.Core.Tracking.State;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Parameters of algorithms changeable by user through user interface
/// </summary>
public class UserDefinedParameters
{
    public VisualisationType VisualisationType { get; set; } = VisualisationType.RawRGB;
    public double ColorCalibrationAdditiveConstant { get; set; } = 0;
    public int MilimetersClippedFromFigure { get; set; } = 10;
    public int NumberOfPointsIndicatingFigure { get; set; } = 2;
    public int MinimalTimeBetweenTrackingTasksInMiliseconds { get; set; } = 20;
    public bool OtzuActiveInBinarization { get; set; } = true;
    public int BinarizationThreshold { get; set; } = 175;
    public bool IsFiguresColorMetricExperimental { get; set; } = false;
    public bool IsDistanceMetricInChessboardFittingExperimental { get; set; } = true;
    public int ClippedDistanecInChessboardFittingMetric { get; set; } = 32;
    public int GameStateInfluenceOnColor { get; set; } = 0;
    public int GameStateInfluenceOnPresence { get; set; } = 0;
    public int InfraredPointFilterThreshold { get; set; } = 1;
    public int DisruptionDetectionThreshold { get; set; } = 315;

    public UserDefinedParameters GetShallowCopy()
    {
        return (UserDefinedParameters)MemberwiseClone();
    }
}
