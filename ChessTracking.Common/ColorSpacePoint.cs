using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    [ProtoContract]
    public struct ColorSpacePoint
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;
    }
}
