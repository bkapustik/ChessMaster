using ChessTracking.Common;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.ImageProcessing.PipelineParts.Stages;
using ChessTracking.Core.ImageProcessing.PipelineParts.StagesInterfaces;
using ChessTracking.Core.Services;
using ChessTracking.Core.Tracking;
using ChessTracking.Core.Tracking.State;
using ChessTracking.Core.Utils;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.General;

/// <summary>
/// Maintains all processing of chessboard tracking
/// </summary>
class Pipeline
{
    private UserDefinedParametersPrototypeFactory UserParametersFactory { get; }

    private IPlaneLocalization PlaneLocalization { get; }
    private IChessboardLocalization ChessboardLocalization { get; }
    private IFiguresLocalization FiguresLocalization { get; }

    private UserDefinedParameters UserParameters { get; set; }
    private DateTime LastReleasedTrackingTask { get; set; } = DateTime.MinValue;

    private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

    public TrackingResultProcessor TrackingProcessor { get; set; }

    public Pipeline(UserDefinedParametersPrototypeFactory userParametersFactory, TrackingResultProcessor trackingProcessor)
    {
        UserParametersFactory = userParametersFactory;
        UserParameters = userParametersFactory.GetShallowCopy();
        PlaneLocalization = new PlaneLocalization(this);
        ChessboardLocalization = new ChessboardLocalization();
        FiguresLocalization = new FiguresLocalization();
        TrackingProcessor = trackingProcessor;
    }

    public void MoveChessboard(ChessboardMovement movement)
    {
        ChessboardLocalization.MoveChessboard(movement);
    }

    public void Reset()
    {
        TrackingProcessor.Reset();
    }

    public bool TryCalibrate(KinectDataClass kinectData)
    {
        try
        {
            UserParameters = UserParametersFactory.GetShallowCopy();

            var inputData = new InputData(kinectData, UserParameters);

            var planeData = PlaneLocalization.Calibrate(inputData);
            var chessboardData = ChessboardLocalization.Calibrate(planeData, out var snapshot);
            TrackingProcessor.SceneCalibrationSnapshotChanged(snapshot);
            var figuresData = FiguresLocalization.Calibrate(chessboardData);
            TrackingProcessor.ChangeProgramState(Services.Events.ProgramState.StartedTracking);

            return true;
        }
        catch (Exception e) 
        {
            TrackingProcessor.RaiseError(e.Message);
            return false;
        }
    }

    public void Process(KinectDataClass kinectData)
    {
        //PipelineSlowdown();

        Semaphore.Wait();

        LastReleasedTrackingTask = DateTime.Now;

        Task.Run(() =>
        {
            try
            {
                ProcessInternal(kinectData);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Semaphore.Release();
            }
        });
    }

    private void ProcessInternal(KinectDataClass kinectData)
    {
        var gameTrackingState = TrackingProcessor.Game.Chessboard.GetTrackingStates();
        gameTrackingState.RotateClockWise(4 - TrackingProcessor.NumberOfCwRotations);
        gameTrackingState.HorizontalFlip();

        UserParameters = UserParametersFactory.GetShallowCopy();
        var inputData = new InputData(kinectData, UserParameters, gameTrackingState);
        var planeData = PlaneLocalization.Track(inputData);
        var chessboardData = ChessboardLocalization.Track(planeData);
        var figuresData = FiguresLocalization.Track(chessboardData);
        var trackingResult = new TrackingResult(
                figuresData.ResultData.VisualisationBitmap.HorizontalFlip(),
                figuresData.ResultData.TrackingState,
                figuresData.ResultData.SceneDisrupted,
                figuresData.ResultData.PointCountsOverFields
        );

        Task.Run(() =>
        {
            TrackingProcessor.ProcessResult(trackingResult);
        });
    }

    /// <summary>
    /// If next release of task is too soon, sleep for reasonable time
    /// </summary>
    private void PipelineSlowdown()
    {
        var milisecondsSinceLastTaskRelease = (DateTime.Now - LastReleasedTrackingTask).Milliseconds;
        if (milisecondsSinceLastTaskRelease < UserParameters.MinimalTimeBetweenTrackingTasksInMiliseconds)
            Thread.Sleep(UserParameters.MinimalTimeBetweenTrackingTasksInMiliseconds - milisecondsSinceLastTaskRelease);
    }
}
