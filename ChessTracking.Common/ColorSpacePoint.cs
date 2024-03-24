using ZeroFormatter;

namespace ChessTracking.Common
{
    [ZeroFormattable]
    public struct ColorSpacePoint
    {
        [Index(0)]
        public float X;

        [Index(1)]
        public float Y;

        public ColorSpacePoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
