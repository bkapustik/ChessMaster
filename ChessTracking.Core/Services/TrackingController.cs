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
    private bool Calibrating { get; set; }
    private bool TrackingCanceled { get; set; }
    private bool IsPaused { get; set; }

    public string KinectTrackingAppPath { get; set; }
    public TrackingResultProcessor TrackingProcessor { get; private set; }
    public SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
    public SharedMemorySerializedMultiBuffer<KinectData> Buffer { get; }

    private readonly Pipeline pipeline;
    private Process? TrackerAppProcess { get; set; } = null;
    public bool CanTakeAnother { get; set; } = true;
    private bool RunningHadBeenStarted { get; set; } = false;

    public TrackingController(UserDefinedParametersPrototypeFactory userParameters, TrackingResultProcessor trackingProcessor)
    {
        KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
            CommonMemoryConstants.KinectInputMessageMemorySize,
            CommonMemoryConstants.KinectInputMessageMemoryFileName,
            CommonMemoryConstants.KinectInputMessageMemoryMutexName,
            useSerializer: true);

        Buffer = new SharedMemorySerializedMultiBuffer<KinectData>(
                 CommonMemoryConstants.BufferMemoryFileName,
                 CommonMemoryConstants.BufferMemoryMutexName,
                 CommonMemoryConstants.BufferMemorySize,
                 CommonMemoryConstants.BufferMaximumRecords,
                 CommonMemoryConstants.NumberOfTasksPerByteArray);

        TrackingProcessor = trackingProcessor;

        pipeline = new Pipeline(userParameters, TrackingProcessor);

        Calibrated = false;
        Calibrating = false;
        TrackingCanceled = false;
        IsPaused = true;
    }

    //TODO return this back
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
            if (!Calibrating)
            {

                GC.Collect();
                Stopwatch sw = Stopwatch.StartNew();

                Buffer.TakeOne(out KinectData kinectData);
                sw.Stop();
                Debug.WriteLine($"Taking one took: {sw.ElapsedMilliseconds} ms");
             

                Task.Run(() =>
                {
                    var kinectDataClass = new KinectDataClass(kinectData);

                    if (!Calibrated)
                    {
                        Calibrating = true;
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
                });
            }
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

        TrackingProcessor.ChangeProgramState(Events.ProgramState.StoppedTracking);
    }

    public void Recalibrate()
    {
        Calibrated = false;

        TrackingProcessor.ChangeProgramState(Events.ProgramState.Recalibrating);
    }

    private void RecalibrateInternal(KinectDataClass kinectData)
    {
        Calibrated = false;
        pipeline.Reset();
        Calibrated = pipeline.TryCalibrate(kinectData);
        Calibrating = false;
    }

    public void Dispose()
    {
        TrackingCanceled = true;
        TrackerAppProcess?.Kill();
        TrackerAppProcess?.Dispose();
    }
}
