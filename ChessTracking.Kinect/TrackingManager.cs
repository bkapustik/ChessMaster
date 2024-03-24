using MemoryMappedCollections;
using ChessTracking.Common;
using System.Threading;
using System;

namespace ChessTracking.Kinect
{
    /// <summary>
    /// Maintains communication with tracking thread
    /// </summary>
    class TrackingManager
    {
        private Kinect Kinect { get; set; }
        private SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
        private SharedMemorySerializedMultiBuffer<KinectData> Buffer { get; }

        public TrackingManager()
        {
            Buffer = new SharedMemorySerializedMultiBuffer<KinectData>(
                CommonMemoryConstants.BufferMemoryFileName,
                CommonMemoryConstants.BufferMemoryMutexName,
                CommonMemoryConstants.BufferMemorySize,
                CommonMemoryConstants.BufferMaximumRecords,
                CommonMemoryConstants.NumberOfTasksPerByteArray);

            KinectInputQueue = new SharedMemoryQueue<KinectInputMessage>(
                CommonMemoryConstants.KinectInputMessageMemorySize,
                CommonMemoryConstants.KinectInputMessageMemoryFileName,
                CommonMemoryConstants.KinectInputMessageMemoryMutexName,
                useSerializer: true);
        }

        public void Run()
        {
            while (true)
            {
                if (KinectInputQueue.GetCount() > 0)
                {
                    KinectInputQueue.TryDequeue(out var inputData);

                    if (inputData.MessageType == KinectInputMessageType.Start)
                    {
                        StartTracking();
                    }

                    if (inputData.MessageType == KinectInputMessageType.Stop)
                    {
                        StopTracking();
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void StartTracking()
        {
            Kinect = new Kinect(Buffer);
        }

        private void StopTracking(bool gameFinished = false)
        {
            if (Kinect != null)
            {
                Kinect.Dispose();
                Kinect = null;
            }
        }
    }
}
