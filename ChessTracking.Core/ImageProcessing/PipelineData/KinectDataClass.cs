using ChessTracking.Common;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

public class KinectDataClass
{
    public byte[] ColorFrameData { get; set; }
    public ushort[] DepthData { get; set; }
    public ushort[] InfraredData { get; set; }
    public CameraSpacePoint[] CameraSpacePointsFromDepthData { get; set; }
    public DepthSpacePoint[] PointsFromColorToDepth { get; set; }
    public ColorSpacePoint[] PointsFromDepthToColor { get; set; }

    public KinectDataClass(KinectData kinectData)
    {
        ColorFrameData = kinectData.ColorFrameData;
        DepthData = kinectData.DepthData;
        InfraredData = kinectData.InfraredData;
        CameraSpacePointsFromDepthData = kinectData.CameraSpacePointsFromDepthData;
        PointsFromColorToDepth = kinectData.PointsFromColorToDepth;
        PointsFromDepthToColor = kinectData.PointsFromDepthToColor;
    }
}
