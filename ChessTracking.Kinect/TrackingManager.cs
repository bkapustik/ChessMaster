﻿using MemoryMappedCollections;
using ChessTracking.Common;
using System.Threading;

namespace ChessTracking.Kinect
{
    /// <summary>
    /// Maintains communication with tracking thread
    /// </summary>
    class TrackingManager
    {
        private Kinect Kinect { get; set; }
        private SharedMemoryQueue<KinectInputMessage> KinectInputQueue { get; }
        private SharedMemoryQueue<KinectData> Buffer { get; }

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

                Thread.Sleep(50);
            }
        }

        private void StartTracking()
        {
            Kinect = new Kinect(Buffer);
        }

        private void StopTracking(bool gameFinished = false)
        {
            Kinect.Dispose();
            Kinect = null;
        }
    }
}
