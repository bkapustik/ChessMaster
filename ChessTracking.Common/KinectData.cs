using ZeroFormatter;

namespace ChessTracking.Common
{
    [ZeroFormattable]
    public struct KinectData
    {
        [Index(0)]
        public byte[] ColorFrameData { get; set; }

        [Index(1)]
        public ushort[] DepthData { get; set; }
        
        [Index(2)]
        public ushort[] InfraredData { get; set; }
        
        [Index(3)]
        public CameraSpacePoint[] CameraSpacePointsFromDepthData { get; set; }
        
        [Index(4)]
        public DepthSpacePoint[] PointsFromColorToDepth { get; set; }
        
        [Index(5)]
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
