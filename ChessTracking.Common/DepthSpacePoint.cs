using ZeroFormatter;

namespace ChessTracking.Common
{
    [ZeroFormattable]
    public struct DepthSpacePoint
    {
        [Index(0)]
        public float X;

        [Index(1)]
        public float Y;

        public DepthSpacePoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
