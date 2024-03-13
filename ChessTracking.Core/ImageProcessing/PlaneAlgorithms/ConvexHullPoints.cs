using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.ImageProcessing.PlaneAlgorithms;

public struct ConvexHullPoints : IComparable<ConvexHullPoints>
{
    public float X;
    public float Y;
    public float Z;

    public PixelType Type;

    public int PositionInBitmap;

    public ConvexHullPoints(ref MyCameraSpacePoint point, int position)
    {
        X = point.X;
        Y = point.Y;
        Z = point.Z;

        Type = point.Type;

        PositionInBitmap = position;
    }

    public int CompareTo(ConvexHullPoints other)
    {
        if (X < other.X)
            return -1;
        if (X > other.X)
            return +1;
        if (Y < other.Y)
            return -1;
        if (Y > other.Y)
            return +1;
        return 0;
    }
}
