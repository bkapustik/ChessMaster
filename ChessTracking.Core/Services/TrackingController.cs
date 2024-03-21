using ChessTracking.Common;
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
    public bool CanTakeAnother { get; set; } = true;

    public TrackingController(UserDefinedParametersPrototypeFactory userParameters, TrackingResultProcessor trackingProcessor)
    {
        KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
            CommonMemoryConstants.KinectInputMessageMemorySize,
            CommonMemoryConstants.KinectInputMessageMemoryFileName,
            CommonMemoryConstants.KinectInputMessageMemoryMutexName);

        Buffer = new SharedMemoryQueue<KinectData>(
            CommonMemoryConstants.BufferMemoryFileName,
            CommonMemoryConstants.BufferMemoryMutexName,
            100000000, true);

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

                var kinectDataClass = new KinectDataClass(kinectData);

                if (!Calibrated)
                {
                    RecalibrateInternal(kinectDataClass);
                }
                else
                {

                    //TODO change maybe
                    ProcessKinectData(kinectDataClass);
                    //if (CanTakeAnother)
                    //{
                    //    ProcessKinectData(kinectDataClass);
                    //    CanTakeAnother = false;
                    //}
                }
            }

            Thread.Sleep(50);
        }
    }

    private void ProcessKinectData(KinectDataClass kinectData)
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

    public void Recalibrate()
    {
        Calibrated = false;
    }

    private void RecalibrateInternal(KinectDataClass kinectData)
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
