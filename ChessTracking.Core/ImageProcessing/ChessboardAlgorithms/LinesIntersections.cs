using MathNet.Spatial.Euclidean;
using Point2D = MathNet.Spatial.Euclidean.Point2D;
using System.Drawing;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms;

/// <summary>
/// Contains algorithm for intersection of two groups of hough lines
/// </summary>
public static class LinesIntersections
{
    /// <summary>
    /// Performs algorithm for intersection of two groups of hough lines
    /// </summary>
    /// <param name="linesTuple">Two groups of lines</param>
    /// <returns>Contracted intersections</returns>
    public static List<Point2D> GetIntersectionPointsOfTwoGroupsOfLines(Tuple<Emgu.CV.Structure.LineSegment2D[], Emgu.CV.Structure.LineSegment2D[]> linesTuple)
    {
        var firstGroup = linesTuple.Item1;
        var secondGroup = linesTuple.Item2;

        var points = GetIntersections(firstGroup, secondGroup);

        // add ending points of lines (they can be involved into edges of chessboard)
        foreach (var line in firstGroup.Concat(secondGroup))
        {
            points.Add(new Point2D(line.P1.X, line.P1.Y));
            points.Add(new Point2D(line.P2.X, line.P2.Y));
        }

        return GetContractedPoints(points);
    }

    /// <summary>
    /// Computes mutual intersections of two groups of lines using library functions
    /// </summary>
    private static List<Point2D> GetIntersections(Emgu.CV.Structure.LineSegment2D[] firstGroup, Emgu.CV.Structure.LineSegment2D[] secondGroup)
    {
        var points = new List<Point2D>();

        // convert groups to library representation of lines, so we can use built-in intersection algorithm
        var firstGroupLibrary =
            firstGroup.Select(line =>
                new Line2D(
                    new Point2D(line.P1.X, line.P1.Y),
                    new Point2D(line.P2.X, line.P2.Y)
                )
            ).ToList();

        var secondGroupLibrary =
            secondGroup.Select(line =>
                new Line2D(
                    new Point2D(line.P1.X, line.P1.Y),
                    new Point2D(line.P2.X, line.P2.Y)
                )
            ).ToList();

        // compute intersections
        foreach (var line1 in firstGroupLibrary)
        {
            foreach (var line2 in secondGroupLibrary)
            {
                var accordLine1 =
                    new LineSegment2DF(
                        new PointF((float)line1.StartPoint.X, (float)line1.StartPoint.Y),
                        new PointF((float)line1.EndPoint.X, (float)line1.EndPoint.Y)
                    );
                var accordLine2 =
                    new LineSegment2DF(
                        new PointF((float)line2.StartPoint.X, (float)line2.StartPoint.Y),
                        new PointF((float)line2.EndPoint.X, (float)line2.EndPoint.Y)
                    );

                var accordNullableIntersectionPoint = accordLine1.GetIntersectionWith(accordLine2);
                if (accordNullableIntersectionPoint != null)
                    points.Add(new Point2D(accordNullableIntersectionPoint.Value.X, accordNullableIntersectionPoint.Value.Y));
            }
        }

        return points;
    }

    /// <summary>
    /// Points that are too close to each other are merged into a single point
    /// </summary>
    private static List<Point2D> GetContractedPoints(List<Point2D> points)
    {
        double maximalContractionDistance = 12; //15

        var contractedPoints = new List<Point2D>();

        while (true)
        {
            // if remaining points list is empty -> break
            if (points.Count == 0)
                break;

            // new list for points to average and add first element from remaining list
            var pointsToContract = new List<Point2D>();
            var processedPoint = points.First();
            points.RemoveAt(0);

            pointsToContract.Add(processedPoint);

            // loop throught remaining list and find close neighbors
            foreach (var point in points)
            {
                double diffX = (processedPoint.X - point.X);
                double diffY = (processedPoint.Y - point.Y);

                if (Math.Sqrt(diffX * diffX + diffY * diffY) < maximalContractionDistance)
                {
                    pointsToContract.Add(point);
                }
            }

            // remove them all from remaining list
            points.RemoveAll(p => pointsToContract.Contains(p));

            // compute average and add it to list
            var x = pointsToContract.Sum(p => p.X);
            var y = pointsToContract.Sum(p => p.Y);
            var count = pointsToContract.Count;

            contractedPoints.Add(new Point2D((int)x / count, (int)y / count));
        }

        return contractedPoints;
    }
}

public class Line
{
    public float Slope { get; private set; }
    public float Intercept { get; private set; }
    public bool IsVertical { get; private set; }

    private Line(float slope, float intercept, bool isVertical)
    {
        Slope = slope;
        Intercept = intercept;
        IsVertical = isVertical;
    }

    public static Line FromPoints(PointF start, PointF end)
    {
        if (start.X == end.X)
        {
            // The line is vertical. Slope is undefined, and intercept is the x-value of the vertical line.
            return new Line(float.NaN, start.X, true);
        }
        else
        {
            float slope = (end.Y - start.Y) / (end.X - start.X);
            float intercept = start.Y - (slope * start.X);
            return new Line(slope, intercept, false);
        }
    }

    public PointF? GetIntersectionWith(Line other)
    {
        if (this.IsVertical && other.IsVertical)
        {
            // Parallel vertical lines do not intersect (unless they are the same line, not covered here).
            return null;
        }
        else if (this.IsVertical)
        {
            // This line is vertical, find intersection on the other line.
            return new PointF(this.Intercept, other.Slope * this.Intercept + other.Intercept);
        }
        else if (other.IsVertical)
        {
            // The other line is vertical, find intersection on this line.
            return new PointF(other.Intercept, this.Slope * other.Intercept + this.Intercept);
        }
        else if (this.Slope == other.Slope)
        {
            // Parallel (and not coincident) lines do not intersect.
            return null;
        }
        else
        {
            // Find intersection of two non-vertical, non-parallel lines.
            float x = (other.Intercept - this.Intercept) / (this.Slope - other.Slope);
            float y = this.Slope * x + this.Intercept;
            return new PointF(x, y);
        }
    }
}

public class LineSegment2DF
{
    private readonly PointF start;
    private readonly PointF end;
    // Assume a compatible Line class exists that can be constructed from PointF and has necessary properties/methods.
    private readonly Line line;

    public PointF Start => start;
    public PointF End => end;

    public LineSegment2DF(PointF start, PointF end)
    {
        // Initialize the line from start and end points (implementation depends on your Line class)
        this.start = start;
        this.end = end;
        this.line = Line.FromPoints(start, end);
    }

    private enum ProjectionLocation
    {
        RayA,
        SegmentAB,
        RayB
    }

    public PointF? GetIntersectionWith(LineSegment2DF other)
    {
        PointF? result = null;

        if (line.Slope == other.line.Slope || (line.IsVertical && other.line.IsVertical))
        {
            if (line.Intercept == other.line.Intercept)
            {
                ProjectionLocation projectionLocation = LocateProjection(other.start);
                ProjectionLocation projectionLocation2 = LocateProjection(other.end);

                if (projectionLocation != ProjectionLocation.SegmentAB && projectionLocation == projectionLocation2)
                {
                    return null;
                }
                else if ((start == other.start && projectionLocation2 == ProjectionLocation.RayA) || (start == other.end && projectionLocation == ProjectionLocation.RayA))
                {
                    return start;
                }
                else if ((end == other.start && projectionLocation2 == ProjectionLocation.RayB) || (end == other.end && projectionLocation == ProjectionLocation.RayB))
                {
                    return end;
                }
                else
                {
                    throw new InvalidOperationException("Overlapping segments do not have a single intersection point.");
                }
            }
        }
        else
        {
            result = line.GetIntersectionWith(other.line); // Assume this method exists and returns PointF?
            if (result.HasValue && other.LocateProjection(result.Value) != ProjectionLocation.SegmentAB)
            {
                return null;
            }
        }

        return result;
    }

    private ProjectionLocation LocateProjection(PointF point)
    {
        PointF direction = new PointF(end.X - start.X, end.Y - start.Y);
        PointF pointToStart = new PointF(point.X - start.X, point.Y - start.Y);
        float dotProduct = pointToStart.X * direction.X + pointToStart.Y * direction.Y;
        float squaredLength = direction.X * direction.X + direction.Y * direction.Y;

        if (dotProduct < 0)
        {
            return ProjectionLocation.RayA;
        }
        else if (dotProduct > squaredLength)
        {
            return ProjectionLocation.RayB;
        }
        else
        {
            return ProjectionLocation.SegmentAB;
        }
    }
}