namespace ChessMaster.RobotDriver.SerialDriver
{
    public class SerialCommand
    {
        public string Command { get; set; }
        public int Timeout { get; set; }
        public int? TimeToExecute { get; set; }

        public SerialCommand()
        {
        }

        public SerialCommand(string command, int? timeToExecute = null, int timeOut = 500000)
        {
            Command = command;
            Timeout = timeOut;
            TimeToExecute = timeToExecute;
        }
    }
}
