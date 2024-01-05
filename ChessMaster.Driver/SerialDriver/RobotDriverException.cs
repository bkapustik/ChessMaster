namespace ChessMaster.RobotDriver.Driver;

public class RobotDriverException : Exception
{
    public readonly int error;

    public RobotDriverException(int error = 0)
    {
        this.error = error;
    }

    public RobotDriverException(string message, int error = 0)
        : base(message)
    {
        this.error = error;
    }

    public RobotDriverException(string message, Exception inner, int error = 0)
        : base(message, inner)
    {
        this.error = error;
    }
}
