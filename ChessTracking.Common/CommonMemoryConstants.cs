using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    public static class CommonMemoryConstants
    {
        public const int KinectOutputMessageMemorySize = 3;
        public const string KinectOutputMessageMemoryFileName = nameof(KinectOutputMessageMemoryFileName);
        public const string KinectOutputMessageMemoryMutexName = nameof(KinectOutputMessageMemoryMutexName);

        public const int KinectInputMessageMemorySize = 3;
        public const string KinectInputMessageMemoryFileName = nameof(KinectInputMessageMemoryFileName);
        public const string KinectInputMessageMemoryMutexName = nameof(KinectInputMessageMemoryMutexName);

        public const int BufferMaximumRecords = 1;
        public const string BufferMemoryFileName = nameof(BufferMemoryFileName);
        public const string BufferMemoryMutexName = nameof(BufferMemoryMutexName);

        public const int BufferMemorySize = 100000000;
    }
}
