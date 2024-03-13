using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Utils;

struct Point
{
    public int X { get; }
    public int Y { get; }
    public int Position { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
        Position = X + Y * PlaneLocalizationConfig.DepthImageWidth;
    }
}
