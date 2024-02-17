using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.Robotic.Events;
using ChessMaster.RobotDriver.SerialResponse;
using System.Diagnostics;
using System.IO.Ports;
using System.Numerics;

namespace ChessMaster.RobotDriver.Driver;

public class SerialDriver : ISerialDriver
{
    private string portName;
    private SerialPort? serialPort = null;
    private List<string> comlog = new List<string>();
    private readonly SerialCommandFactory commandFactory;
    private int lastAlarm = 0;
    public bool HomingRequired { get; private set; }
    public CommandsCompletedEvent? CommandsExecuted { get; set; }

    public SerialDriver(string portName)
    {
        this.portName = portName;
        commandFactory = new SerialCommandFactory();
    }
    
    public Vector3 GetOrigin()
    {
        SerialWriteLine(commandFactory.Info().Command);
        string response;
        do
        {
            response = SerialReadLine();
            if (response.StartsWith(RobotDriverResponse.ORIGIN_COORDINATES))
            {
                var tokens = response.Substring(5, response.Length - 7).Split(',');

                var origin = new Vector3();
                origin.X = float.Parse(tokens[0], System.Globalization.CultureInfo.InvariantCulture);
                origin.Y = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                origin.Z = float.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
                return origin;
            }
        } while (!response.StartsWith(RobotDriverResponse.OK));

        throw new RobotDriverException("Origin coordinates missing in info response!");
    }
    public void Initialize()
    {
        if (serialPort != null)
        {
            return;
        }

        serialPort = new SerialPort(portName);
        serialPort.BaudRate = 115200;
        serialPort.ReadTimeout = 5000;
        serialPort.WriteTimeout = 5000;
        serialPort.Open();
        Task.Delay(3000);
        
        SerialWriteLine(commandFactory.Reset().Command);
        Task.Delay(500);

        var response = SerialReadLine();

        while (response != RobotDriverResponse.GENERAL_OK) 
        {
            if (response == RobotDriverResponse.HOMING_REQUIRED)
            {
                HomingRequired = true;
                return;
            }
            response = SerialReadLine();
        }
    }
    public void SetMovementType(SerialCommand movementCommand)
    {
        Task.Delay(100);
        TrySendCommand(movementCommand);

        Task.Delay(100);
        while (serialPort.BytesToRead > 0)
        {
            SerialReadLine();
            Task.Delay(50);
        }
    }
    public void Reset()
    {
        char[] cmd = { (char)0x18 };
        SerialWriteLine(new string(cmd));
        Task.Delay(1000);
        SerialWriteLine(commandFactory.Reset().Command);
        Task.Delay(500);
        while (SerialReadLine() != RobotDriverResponse.GENERAL_OK) { }
    }
    public RobotRawState GetRawState()
    {
        var stateCommand = commandFactory.State();
        string rawState = SendCommandGetResponse(stateCommand);
        while (rawState.StartsWith(RobotDriverResponse.OK))
        {
            rawState = GetResponse(100);
        }
        if (!rawState.StartsWith("<"))
        {
            throw new RobotDriverException("Unexpected response for status request: " + rawState);
        }

        var tokens = rawState.Substring(1).Split(RobotDriverResponse.RESPONSE_SPLITTER);
        if (tokens.Length < 2 || tokens[0] == "" || !tokens[1].StartsWith(RobotDriverResponse.COORDINATES))
        {
            throw new RobotDriverException("Unexpected response for status request: " + rawState);
        }

        var coords = tokens[1].Substring(5).Split(',');
        if (coords.Length != 3)
        {
            throw new RobotDriverException("Unexpected response for status request: " + rawState);
        }
        return new RobotRawState()
        {
            MovementState = tokens[0],
            Coordinates = coords
        };
    }
    protected virtual void OnCommandsExecuted(RobotEventArgs e)
    {
        CommandsExecuted?.Invoke(this, e);
    }
    private string SendCommandGetResponse(SerialCommand command)
    {
        // lingering info is removed
        while (serialPort.BytesToRead > 0)
        {
            SerialReadLine();
            Task.Delay(1);
        }

        SerialWriteLine(command.Command);
        string response = GetResponse(command.Timeout);
        CheckErrorResponse(response, command.Command);
        if (command.TimeToExecute != null)
        {
            Task.Delay(command.TimeToExecute.Value);
        }
        return response;
    }
    private string GetResponse(long timeout)
    {
        var stopwatch = new Stopwatch();
        while (true)
        {
            while (serialPort.BytesToRead == 0)
            {
                stopwatch.Restart();
                Task.Delay(1);
                stopwatch.Stop();
                timeout -= stopwatch.ElapsedMilliseconds;
                if (timeout <= 0)
                {
                    throw new RobotDriverException("Reading operation timed out.");
                }
            }

            string response = SerialReadLine();
            if (response.StartsWith(RobotDriverResponse.HELP_MESSAGE)
                || response == RobotDriverResponse.NEW_LINE) continue; // skip help/notification messages and empty lines

            if (response.EndsWith(RobotDriverResponse.NEW_LINE))
            {
                response = response.Substring(0, response.Length - 1);
            }

            if (response.StartsWith(RobotDriverResponse.ALARM))
            {
                if (!Int32.TryParse(response.Substring(RobotDriverResponse.ALARM.Length), out lastAlarm))
                {
                    lastAlarm = -1;
                }
                continue;
            }

            return response;
        }

        throw new RobotDriverException("Infinite loop ended.");
    }
    private string SerialReadLine()
    {
        string response = serialPort.ReadLine();
        if (comlog != null)
        {
            comlog.Add("> " + response);
        }
        return response;
    }
    private void SerialWriteLine(string line)
    {
        serialPort.WriteLine(line);
        if (comlog != null)
        {
            comlog.Add("< " + line);
        }
    }
    private void CheckErrorResponse(string response, string command)
    {
        int error = 0;
        var tokens = response.Split(':');
        if (tokens.Length == 2 && tokens[0] == RobotDriverResponse.ERROR && Int32.TryParse(tokens[1], out error))
        {
            throw new RobotDriverException("Command '" + command + "' execution failed on error.", error);
        }
    }
    private void SendCommand(SerialCommand command)
    {
        string response = SendCommandGetResponse(command);

        if (!response.StartsWith(RobotDriverResponse.OK))
        {
            throw new RobotDriverException("Command '" + command + "' execution failed on unknown error: " + response);
        }
    }
    public bool TrySendCommand(SerialCommand command)
    {
        if (HomingRequired)
        {
            return false;
        }

        SendCommand(command);
        
        return true;
    }
    public void Home()
    {
        SendCommand(new HomeCommand().GetSerialCommand());

        HomingRequired = false;
    }
}