using ChessTracking.Common;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.ImageProcessing.PipelineParts.General;
using ChessTracking.Core.Tracking.State;
using MemoryMappedCollections;
using System.Diagnostics;
namespace ChessTracking.Core.Services;

public class TrackingController : IDisposable
{
    /// <summary>
    /// Indicated whether figures are located
    /// </summary>
    private bool Calibrated { get; set; }
    private bool TrackingCanceled { get; set; }
    private bool IsPaused { get; set; }

    public string KinectTrackingAppPath { get; set; }
    public TrackingResultProcessor TrackingProcessor { get; private set; }
    public SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
    public SharedMemoryQueue<KinectData> Buffer { get; }

    private readonly Pipeline pipeline;
    private Process? TrackerAppProcess { get; set; } = null;
    public bool CanTakeAnother { get; set; } = true;
    private bool RunningHadBeenStarted { get; set; } = false;

    public TrackingController(UserDefinedParametersPrototypeFactory userParameters, TrackingResultProcessor trackingProcessor)
    {
        KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
            CommonMemoryConstants.KinectInputMessageMemorySize,
            CommonMemoryConstants.KinectInputMessageMemoryFileName,
            CommonMemoryConstants.KinectInputMessageMemoryMutexName);

        Buffer = new SharedMemoryQueue<KinectData>(
            CommonMemoryConstants.BufferMemoryFileName,
            CommonMemoryConstants.BufferMemoryMutexName,
            CommonMemoryConstants.BufferMemorySize, true);

        TrackingProcessor = trackingProcessor;

        pipeline = new Pipeline(userParameters, TrackingProcessor);

        Calibrated = false;
        TrackingCanceled = false;
        IsPaused = true;
    }

    public void StartTrackerApp() 
    {
        if (TrackerAppProcess == null || TrackerAppProcess.HasExited)
        {
            TrackerAppProcess = new Process();
            TrackerAppProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(KinectTrackingAppPath);
            TrackerAppProcess.StartInfo.FileName = KinectTrackingAppPath;
            TrackerAppProcess.StartInfo.Verb = "runas";
            TrackerAppProcess.StartInfo.CreateNoWindow = true;

            TrackerAppProcess.Start();
        }
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

            Thread.Sleep(30);
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
        StartTrackerApp();

        if (!RunningHadBeenStarted)
        {
            RunningHadBeenStarted = true;
            Task.Run(Run);
        }

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
        TrackerAppProcess?.Kill();
        TrackerAppProcess?.Dispose();
    }
}
