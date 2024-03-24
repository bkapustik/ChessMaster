using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    [ProtoContract]
    public struct KinectInputMessage
    {
        [ProtoMember(1)]
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
