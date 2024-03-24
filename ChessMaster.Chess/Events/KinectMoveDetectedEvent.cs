using ChessTracking.Core.Game;

namespace ChessMaster.ChessDriver.Events;

public delegate void KinectMoveDetectedEvent(object o, KinectMoveDetectedEventArgs e);

public class KinectMoveDetectedEventArgs
{
    public string UciMoveString { get; set; }
    public GameMove GameMove { get; set; }
    public KinectMoveDetectedEventArgs(string moveMessage, GameMove gameMove)
    {
        GameMove = gameMove;
        UciMoveString = moveMessage;
    }
}
