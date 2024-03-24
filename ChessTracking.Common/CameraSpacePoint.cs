using ZeroFormatter;

namespace ChessTracking.Common
{
    [ZeroFormattable]
    public struct CameraSpacePoint
    {
        [Index(0)]
        public float X;

        [Index(1)]
        public float Y;

        [Index(2)]
        public float Z;

        public CameraSpacePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
