using ChessTracking.Kinect.Messages;
using MemoryMappedCollections;
using ChessTracking.Common;

namespace ChessTracking.Kinect
{
    /// <summary>
    /// Maintains communication with tracking thread
    /// </summary>
    class TrackingManager
    {
        private Kinect Kinect { get; set; }
        public SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
        public SharedMemoryQueue<KinectData> Buffer { get; }

        public TrackingManager()
        {
            Buffer = new SharedMemoryQueue<KinectData>(
                CommonMemoryConstants.BufferSize,
                CommonMemoryConstants.BufferMemoryFileName,
                CommonMemoryConstants.BufferMemoryMutexName);

            KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
                CommonMemoryConstants.KinectInputMessageMemorySize,
                CommonMemoryConstants.KinectInputMessageMemoryFileName,
                CommonMemoryConstants.KinectInputMessageMemoryMutexName);
        }

        //
        //TODO - smycka kt. bude citat z KinectInputQueue
        //
        //
        //
        //
        //
        //

        public void StartTracking()
        {
            Kinect = new Kinect(Buffer);
        }

        public void StopTracking(bool gameFinished = false)
        {
            Kinect.Dispose();
            Kinect = null;
        }

        public void SendChessboardMovement(ChessboardMovement movement)
        {
            //TODO add to ProcessingOutputQueue
        }
    }
}
