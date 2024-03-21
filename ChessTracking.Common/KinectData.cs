using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    [ProtoContract]
    public struct KinectData
    {
        [ProtoMember(1)]
        public byte[] ColorFrameData { get; set; }

        [ProtoMember(2)]
        public ushort[] DepthData { get; set; }
        
        [ProtoMember(3)]
        public ushort[] InfraredData { get; set; }
        
        [ProtoMember(4)]
        public CameraSpacePoint[] CameraSpacePointsFromDepthData { get; set; }
        
        [ProtoMember(5)]
        public DepthSpacePoint[] PointsFromColorToDepth { get; set; }
        
        [ProtoMember(6)]
        public ColorSpacePoint[] PointsFromDepthToColor { get; set; }

        public KinectData(
            byte[] colorFrameData,
            ushort[] depthData,
            ushort[] infraredData,
            CameraSpacePoint[] cameraSpacePointsFromDepthData,
            DepthSpacePoint[] pointsFromColorToDepth,
            ColorSpacePoint[] pointsFromDepthToColor
        )
        {
            ColorFrameData = colorFrameData;
            DepthData = depthData;
            InfraredData = infraredData;
            CameraSpacePointsFromDepthData = cameraSpacePointsFromDepthData;
            PointsFromColorToDepth = pointsFromColorToDepth;
            PointsFromDepthToColor = pointsFromDepthToColor;
        }
    }
}
