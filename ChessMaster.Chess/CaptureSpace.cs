using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver;

public class CaptureSpace
{
    public Space.Space Space { get; set; }
    private SpacePosition lastCapturePosition;
    public CaptureSpace(Space.Space space)
    {
        Space = space;
        lastCapturePosition = new SpacePosition(0, 0);
    }
    public SpacePosition GetNextFreeSpace()
    {
        if (lastCapturePosition.X == 0 && lastCapturePosition.Y == 0)
        {
            return lastCapturePosition;
        }

        if (lastCapturePosition.Y == Space.Height)
        {
            lastCapturePosition.Y = 0;
            lastCapturePosition.X++;
        }
        else
        {
            lastCapturePosition.Y++;
        }

        return lastCapturePosition;
    }

    public SpacePosition GetLastCapture() => lastCapturePosition;
}
