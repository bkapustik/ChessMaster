using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    [ProtoContract]
    public struct CameraSpacePoint
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;
    }
}
