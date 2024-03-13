using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    public struct KinectInputMessage
    {
        public KinectInputMessageType MessageType { get; set; }
        public KinectInputMessage(KinectInputMessageType messageType)
        {
            MessageType = messageType;
        }
    }

    public enum KinectInputMessageType
    { 
        Start,
        Stop
    }
}
