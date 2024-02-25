namespace ChessMaster.RobotDriver.Events;

public delegate void MessageLoggedEvent(object? o, LogEventArgs e);

public class LogEventArgs : EventArgs
{
    public string Message { get; set; }

    public LogEventArgs(string message) => Message = message;
}