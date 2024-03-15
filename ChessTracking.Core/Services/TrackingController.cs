using ChessTracking.Common;
using ChessTracking.Core.Game;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.ImageProcessing.PipelineParts.General;
using ChessTracking.Core.Tracking.State;
using MemoryMappedCollections;
namespace ChessTracking.Core.Services;

public class TrackingController : IDisposable
{
    /// <summary>
    /// Indicated whether figures are located
    /// </summary>
    private bool Calibrated { get; set; }
    private bool TrackingCanceled { get; set; }
    private bool IsPaused { get; set; }

    public TrackingResultProcessor TrackingProcessor { get; private set; }
    public SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
    public SharedMemoryQueue<KinectData> Buffer { get; }

    private readonly Pipeline pipeline;

    public TrackingController(UserDefinedParametersPrototypeFactory userParameters, TrackingResultProcessor trackingProcessor)
    {
        KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
            CommonMemoryConstants.KinectInputMessageMemorySize,
            CommonMemoryConstants.KinectInputMessageMemoryFileName,
            CommonMemoryConstants.KinectInputMessageMemoryMutexName);

        Buffer = new SharedMemoryQueue<KinectData>(
            CommonMemoryConstants.KinectInputMessageMemorySize,
            CommonMemoryConstants.KinectInputMessageMemoryFileName,
            CommonMemoryConstants.KinectInputMessageMemoryMutexName);

        TrackingProcessor = trackingProcessor;

        pipeline = new Pipeline(userParameters, TrackingProcessor);

        Calibrated = false;
        TrackingCanceled = false;
        IsPaused = true;

        Task.Run(Run);
    }

    private void Run()
    {
        while (!TrackingCanceled)
        {
            if (Buffer.GetCount() > 0)
            {
                Buffer.TryDequeue(out KinectData kinectData);

                if (!Calibrated)
                {
                    RecalibrateInternal(kinectData);
                }
                else
                {
                    ProcessKinectData(kinectData);
                }
            }

            Thread.Sleep(50);
        }
    }

    private void ProcessKinectData(KinectData kinectData)
    {
        pipeline.Process(kinectData);
    }

    public void SendChessboardMovement(ChessboardMovement movement)
    {
        pipeline.MoveChessboard(movement);
    }

    public void Start()
    {
        var message = new KinectInputMessage(KinectInputMessageType.Start);
        KinectInputQueue.TryEnqueue(ref message);
    }

    public void Stop()
    {
        var message = new KinectInputMessage(KinectInputMessageType.Stop);
        KinectInputQueue.TryEnqueue(ref message);
        TrackingProcessor.Reset();
    }

    public bool Recalibrate()
    {
        if (Buffer.GetCount() == 0)
        {
            return false;
        }

        Buffer.TryDequeue(out var kinectData);

        RecalibrateInternal(kinectData);
        return true;
    }

    private void RecalibrateInternal(KinectData kinectData)
    {
        Calibrated = false;
        pipeline.Reset();
        Calibrated = pipeline.TryCalibrate(kinectData);
    }

    public void Dispose()
    {
        TrackingCanceled = true;
    }
}
