using System.Drawing;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

public struct Point2DWithColor
{
    public Color Color { get; }
    public int X { get; }
    public int Y { get; }

    public Point2DWithColor(Color color, int x, int y)
    {
        Color = color;
        X = x;
        Y = y;
    }
}
