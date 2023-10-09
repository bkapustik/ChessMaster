using ChessMaster.CommandFactory;
using ChessMaster.Robot.Driver;
using ChessMaster.Robot.State;
using System.Numerics;

namespace ChessMaster.Robot.Robot
{
    public class Robot : IRobot
    {
        private readonly ICommandFactory commands;
        private readonly ISerialDriver driver;
        private Vector3 origin;

        private const float safePadding = 5f;

        public Robot(ICommandFactory commands, ISerialDriver robotDriver)
        {
            this.commands = commands;
            driver = robotDriver;
        }

        public Vector3 Limits
        {
            get => new Vector3(-origin.X - safePadding, -origin.Y - safePadding, -origin.Z - safePadding); 
        } 

        public async Task Home()
        {
            await driver.SendCommand(commands.MoveHome());
        }

        public async Task Initialize()
        {
            await driver.Initialize();
            origin = driver.GetOrigin();
            await driver.SetMovementType(commands.LinearMovement());
        }

        public async Task Reset()
        {
            await driver.Reset();
        }

        public async Task Move(float x, float y, float z)
        {
            await driver.SendCommand(commands.Move(x, y, z));
        }

        public async Task MoveXY(float x, float y)
        {
            await driver.SendCommand(commands.MoveXY(x, y));
        }

        public async Task MoveX(float x)
        {
            await driver.SendCommand(commands.MoveX(x));
        }

        public async Task MoveY(float y)
        {
            await driver.SendCommand(commands.MoveY(y));
        }

        public async Task MoveZ(float z)
        {
            await driver.SendCommand(commands.MoveZ(z));
        }

        public async Task OpenGrip()
        {
            await driver.SendCommand(commands.OpenGrip());
        }

        public async Task CloseGrip()
        {
            await driver.SendCommand(commands.CloseGrip());
        }

        public async Task<RobotState> GetState()
        {
            var rawState = await driver.GetRawState();

            MovementState state = rawState.MovementState.ToMovementState();

            var x = float.Parse(rawState.Coordinates[0], System.Globalization.CultureInfo.InvariantCulture) - origin.X;
            var y = float.Parse(rawState.Coordinates[1], System.Globalization.CultureInfo.InvariantCulture) - origin.Y;
            var z = float.Parse(rawState.Coordinates[2], System.Globalization.CultureInfo.InvariantCulture) - origin.Z;

            return new RobotState(state, x, y, z);
        }

        public async Task Pause()
        {
            await driver.SendCommand(commands.Pause());
            await Task.Delay(1000);
        }

        public async Task Resume()
        {
            await driver.SendCommand(commands.Resume());
        }

        public async Task Stop()
        {
            await Pause();
            await Resume();
        }
    }
}
