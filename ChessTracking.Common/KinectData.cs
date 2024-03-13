using System;
using System.Collections.Generic;
using System.Text;

namespace ChessTracking.Common
{
    public struct KinectData
    {
        public byte[] ColorFrameData { get; set; }
        public ushort[] DepthData { get; set; }
        public ushort[] InfraredData { get; set; }
        public CameraSpacePoint[] CameraSpacePointsFromDepthData { get; set; }
        public DepthSpacePoint[] PointsFromColorToDepth { get; set; }
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
