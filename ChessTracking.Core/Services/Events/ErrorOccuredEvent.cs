namespace ChessTracking.Core.Services.Events;

public delegate void ErrorOccuredEvent(object? o, ErrorOccuredEventArgs e);

public class ErrorOccuredEventArgs : EventArgs
{
    public string Message { get; set; }
    public ErrorOccuredEventArgs(string message)
    {
        Message = message;
    }
}