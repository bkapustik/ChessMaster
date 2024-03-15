namespace ChessTracking.Kinect
{
    public class Program
    {
        static void Main(string[] args)
        {
            var trackingManager = new TrackingManager();
            trackingManager.Run();
        }
    }
}
