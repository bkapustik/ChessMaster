namespace ChessMaster.Robot.SerialResponse;

public static class RobotDriverResponse
{
    public const string OK = "ok";
    public const string GENERAL_OK = "ok\r";
    public const string NEW_LINE = "\n";
    public const string ERROR = "error";
    public const string ORIGIN_COORDINATES = "[G54:";
    public const string ALARM = "ALARM:";
    public const string HELP_MESSAGE = "[";
    public const string RESPONSE_SPLITTER = "|";
    public const string COORDINATES = "MPos:";
}
