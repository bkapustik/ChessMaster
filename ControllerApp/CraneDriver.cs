using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Numerics;

namespace ControllerApp
{
    public enum CraneState
    {
        Idle,
        Run,
        Hold,
        Unknown
    }

    public struct CraneInfo
    {
        public readonly CraneState State;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3 Position { get { return new Vector3(X, Y, Z); } }

        public CraneInfo(CraneState state, float x, float y, float z)
        {
            State = state;
            X = x;
            Y = y;
            Z = z;
        }
    }

    public interface ICraneDriver
    {
        Vector3 Limits { get; }

        Task Initialize();

        Task Reset();

        Task Home();

        Task Move(float x, float y, float z);

        Task MoveXY(float x, float y);

        Task MoveX(float x);

        Task MoveY(float y);

        Task MoveZ(float z);

        Task OpenGrip();

        Task CloseGrip();

        Task<CraneInfo> GetState();

        Task Pause();

        Task Resume();

        Task Stop();
    }

    public class CraneDriver : ICraneDriver
    {
        private string portName;

        /// <summary>
        /// Serial port connection object.
        /// </summary>
        private SerialPort serialPort = null;
        private List<string> comlog = new List<string>();

        /// <summary>
        /// Alarms are written out to console ad-hoc, this is the last parsed alarm value.
        /// </summary>
        private int lastAlarm = 0;

        float originX, originY, originZ;
        // note -5f is safety padding
        public Vector3 Limits { get { return new Vector3(-originX - 5f, -originY - 5f, -originZ - 5f); } }

        private void CheckErrorResponse(string response, string command)
        {
            int error = 0;
            var tokens = response.Split(':');
            if (tokens.Length == 2 && tokens[0] == "error" && Int32.TryParse(tokens[1], out error))
            {
                throw new CraneDriverException("Command '" + command + "' execution failed on error.", error);
            }
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
                        throw new CraneDriverException("Reading operation timed out.");
                    }
                }

                string response = SerialReadLine();
                if (response.StartsWith("[") || response == "\r") continue; // skip help/notification messages and empty lines

                if (response.EndsWith("\r"))
                {
                    response = response.Substring(0, response.Length - 1);
                }

                if (response.StartsWith("ALARM:"))
                {
                    if (!Int32.TryParse(response.Substring("ALARM:".Length), out lastAlarm))
                    {
                        lastAlarm = -1;
                    }
                    continue;
                }

                return response;
            }

            throw new CraneDriverException("Infinite loop ended.");
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


        private async Task SendCommand(string command, long timeout = 5000)
        {
            string response = await SendCommandGetResponse(command, timeout);
            if (response != "ok")
            {
                throw new CraneDriverException("Command '" + command + "' execution failed on unknown error: " + response);
            }
        }

        private void ReadInfo()
        {
            SerialWriteLine("$#");
            string response;
            bool loaded = false;
            do
            {
                response = SerialReadLine();
                if (response.StartsWith("[G54:"))
                {
                    var tokens = response.Substring(5, response.Length - 7).Split(',');
                    originX = float.Parse(tokens[0], System.Globalization.CultureInfo.InvariantCulture);
                    originY = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                    originZ = float.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
                    loaded = true;
                }
            } while (!response.StartsWith("ok"));

            if (!loaded)
            {
                throw new CraneDriverException("Origin coordinates G54 missing in $# response!");
            }
        }


        public CraneDriver(string portName)
        {
            this.portName = portName;

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

            SerialWriteLine("$X");
            await Task.Delay(500);
            while (SerialReadLine() != "ok\r") {} // also skip initial welcome lines

            ReadInfo();
            await Task.Delay(100);
            await SendCommand("G00");

            await Task.Delay(100);

            // lingering info is removed
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
            SerialWriteLine("$X");
            await Task.Delay(500);
            while (SerialReadLine() != "ok\r") { } // also skip initial welcome lines
        }

        public async Task Home()
        {
            await SendCommand("$H", 300000); // homing can take a while to process
        }

        private static string F2S(float val)
        {
            long rounded = (long)Math.Round(val * 1000);
            long i = rounded / 1000;
            long frac = Math.Abs(rounded) % 1000;
            return string.Format("{0}.{1}", i, frac);
        }

        public Task Move(float x, float y, float z)
        {
            return SendCommand(string.Format("X{0} Y{1} Z{2}", F2S(x), F2S(y), F2S(z)));
        }

        public Task MoveXY(float x, float y)
        {
            return SendCommand(string.Format("X{0} Y{1}", F2S(x), F2S(y)));
        }

        public Task MoveX(float x)
        {
            return SendCommand(string.Format("X{0}", F2S(x)));
        }

        public Task MoveY(float y)
        {
            return SendCommand(string.Format("Y{0}", F2S(y)));
        }

        public Task MoveZ(float z)
        {
            return SendCommand(string.Format("Z{0}", F2S(z)));
        }

        public async Task OpenGrip()
        {
            await SendCommand("M8");
            await Task.Delay(500);
        }

        public async Task CloseGrip()
        {
            await SendCommand("M9");
            await Task.Delay(500);
        }

        private static readonly Dictionary<string, CraneState> stateIdentifiers = new Dictionary<string, CraneState>
        {
            { "Idle", CraneState.Idle },
            { "Run", CraneState.Run },
            { "Hold", CraneState.Hold },
        };
        public async Task<CraneInfo> GetState()
        {
            string rawState = await SendCommandGetResponse("?", 500);
            while (rawState == "ok")
            {
                rawState = await GetResponse(100);
            }
            if (!rawState.StartsWith("<"))
            {
                throw new CraneDriverException("Unexpected response for status request: " + rawState);
            }

            var tokens = rawState.Substring(1).Split('|');
            if (tokens.Length < 2 || tokens[0] == "" || !tokens[1].StartsWith("MPos:"))
            {
                throw new CraneDriverException("Unexpected response for status request: " + rawState);
            }

            CraneState state = CraneState.Unknown;
            if (stateIdentifiers.ContainsKey(tokens[0]))
            {
                state = stateIdentifiers[tokens[0]];
            }

            var coords = tokens[1].Substring(5).Split(',');
            if (coords.Length != 3)
            {
                throw new CraneDriverException("Unexpected response for status request: " + rawState);
            }

            var x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture) - originX;
            var y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture) - originY;
            var z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture) - originZ;

            return new CraneInfo(state, x, y, z);
        }

        public async Task Pause()
        {
            await SendCommand("!");
            await Task.Delay(1000);
        }

        public Task Resume()
        {
            return SendCommand("~");
        }

        public async Task Stop()
        {
            await Pause();
            await Reset();
        }
    }

    public class FakeCraneDriver : ICraneDriver
    {
        private bool running = false;
        private Vector3 position = new Vector3(0f, 0f, 0f);
        private float originX = -490f, originY = -820f, originZ = -200f;
        public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }


        public FakeCraneDriver()
        {
        }

        public async Task Initialize()
        {
            await Task.Delay(1000);
        }

        public async Task Reset()
        {
            await Task.Delay(1000);
        }

        public async Task Home()
        {
            await Task.Delay(5000);
        }

        public async Task Move(float x, float y, float z)
        {
            await Task.Delay(50);
        }

        public async Task MoveXY(float x, float y)
        {
            await Task.Delay(50);
        }

        public async Task MoveX(float x)
        {
            await Task.Delay(50);
        }

        public async Task MoveY(float y)
        {
            await Task.Delay(50);
        }

        public async Task MoveZ(float z)
        {
            await Task.Delay(50);
        }

        public async Task OpenGrip()
        {
            await Task.Delay(50);
        }

        public async Task CloseGrip()
        {
            await Task.Delay(50);
        }

        public async Task<CraneInfo> GetState()
        {
            await Task.Delay(50);
            return new CraneInfo(CraneState.Idle, position.X, position.Y, position.Z);
        }

        public async Task Pause()
        {
            await Task.Delay(50);
        }

        public async Task Resume()
        {
            await Task.Delay(50);
        }

        public async Task Stop()
        {
            await Pause();
            await Reset();
        }

    }

    public class CraneDriverException : Exception
    {
        public readonly int error;

        public CraneDriverException(int error = 0)
        {
            this.error = error;
        }

        public CraneDriverException(string message, int error = 0)
            : base(message)
        {
            this.error = error;
        }

        public CraneDriverException(string message, Exception inner, int error = 0)
            : base(message, inner)
        {
            this.error = error;
        }
    }
}
