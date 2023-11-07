using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.SerialResponse;
using ChessMaster.RobotDriver.State;
using System.Diagnostics;
using System.IO.Ports;
using System.Numerics;

namespace ChessMaster.RobotDriver.Driver;

public class SerialDriver : ISerialDriver
{
    private string portName;
    private SerialPort serialPort = null;
    private List<string> comlog = new List<string>();

    private Queue<string> commandQueue = new Queue<string>();

    /// <summary>
    /// Alarms are written out to console ad-hoc, this is the last parsed alarm value.
    /// </summary>
    private int lastAlarm = 0;

    private readonly SerialCommandFactory commandFactory;

    public SerialDriver(string portName)
    {
        this.portName = portName;
        commandFactory = new SerialCommandFactory();
    }

    public CommandsCompletedEvent CommandsExecuted { get; set; }

    public Vector3 GetOrigin()
    {
        SerialWriteLine(commandFactory.Info());
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
    public void ScheduleCommand(string command)
    {
        commandQueue.Enqueue(command);
        if (commandQueue.Count == 1)
        {
            Task.Run(ExecuteCommands);
        }
    }
    public async Task Initialize()
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
        await Task.Delay(3000);

        SerialWriteLine(commandFactory.Reset());
        await Task.Delay(500);
        while (SerialReadLine() != RobotDriverResponse.GENERAL_OK) { }
    }
    public async Task SetMovementType(string movementCommand)
    {
        await Task.Delay(100);
        await SendCommand(movementCommand);

        await Task.Delay(100);
        while (serialPort.BytesToRead > 0)
        {
            SerialReadLine();
            await Task.Delay(50);
        }
    }
    public async Task Reset()
    {
        char[] cmd = { (char)0x18 };
        SerialWriteLine(new string(cmd));
        await Task.Delay(1000);
        SerialWriteLine(commandFactory.Reset());
        await Task.Delay(500);
        while (SerialReadLine() != RobotDriverResponse.GENERAL_OK) { }
    }
    public async Task<RobotRawState> GetRawState()
    {
        string rawState = await SendCommandGetResponse(commandFactory.State(), 500);
        while (rawState == RobotDriverResponse.OK)
        {
            rawState = await GetResponse(100);
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

    private async Task<string> SendCommandGetResponse(string command, long timeout = 5000)
    {
        // lingering info is removed
        while (serialPort.BytesToRead > 0)
        {
            SerialReadLine();
            await Task.Delay(1);
        }

        SerialWriteLine(command);
        string response = await GetResponse(timeout);
        CheckErrorResponse(response, command);
        return response;
    }
    private async Task<string> GetResponse(long timeout = 5000)
    {
        var stopwatch = new Stopwatch();
        while (true)
        {
            while (serialPort.BytesToRead == 0)
            {
                stopwatch.Restart();
                await Task.Delay(1);
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
    private async Task SendCommand(string command, long timeout = 5000)
    {
        string response = await SendCommandGetResponse(command, timeout);
        if (response != RobotDriverResponse.OK)
        {
            throw new RobotDriverException("Command '" + command + "' execution failed on unknown error: " + response);
        }
    }
    private async Task ExecuteCommands()
    {
        while (commandQueue.Count > 0)
        {
            await SendCommandAtLastCompletion(commandQueue.Dequeue());
        }

        OnCommandsExecuted(new RobotEventArgs(success: true, new RobotState()));
    }
    private async Task SendCommandAtLastCompletion(string command)
    {
        bool isIdle = false;
        while (!isIdle)
        {
            var currentState = await GetRawState();
            isIdle = currentState.MovementState == MovementState.Idle.ToString();

            if (!isIdle)
            {
                await Task.Delay(10);
            }
            else
            {
                await SendCommand(command);
            }
        }
    }
}
