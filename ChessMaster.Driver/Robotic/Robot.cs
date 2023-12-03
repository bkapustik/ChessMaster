using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic
{
    public class Robot : IRobot
    {
        private readonly SerialCommandFactory commands;
        private readonly ISerialDriver driver;

        private Vector3 origin;

        private const float safePadding = 5f;

        public Robot(ISerialDriver robotDriver)
        {
            commands = new SerialCommandFactory();
            driver = robotDriver;
        }

        public void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
        {
            driver.CommandsExecuted += e; 
        }

        public async Task Initialize()
        {
            await driver.Initialize();
            
            origin = driver.GetOrigin();
            await driver.SetMovementType(commands.LinearMovement());
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

        public Vector3 Limits
        {
            get => new Vector3(-origin.X - safePadding, -origin.Y - safePadding, -origin.Z - safePadding);
        }

        public void Home()
        {
            driver.ScheduleCommand(commands.MoveHome());
        }
        public void Reset()
        {
            driver.Reset();
        }
        public void Move(float x, float y, float z)
        {
            driver.ScheduleCommand(commands.Move(x, y, z));
        }
        public void Move(Vector3 targetPosition)
        {
            Move(targetPosition.X, targetPosition.Y, targetPosition.Z);
        }
        public void MoveXY(float x, float y)
        {
            driver.ScheduleCommand(commands.MoveXY(x, y));
        }
        public void MoveX(float x)
        {
            driver.ScheduleCommand(commands.MoveX(x));
        }
        public void MoveY(float y)
        {
            driver.ScheduleCommand(commands.MoveY(y));
        }
        public void MoveZ(float z)
        {
            driver.ScheduleCommand(commands.MoveZ(z));
        }
        public void OpenGrip()
        {
            driver.ScheduleCommand(commands.OpenGrip());
        }
        public void CloseGrip()
        {
            driver.ScheduleCommand(commands.CloseGrip());
        }
        public void Pause()
        {
            driver.ScheduleCommand(commands.Pause());
        }
        public void Resume()
        {
            driver.ScheduleCommand(commands.Resume());
        }
        public void Stop()
        {
            Pause();
            Resume();
        }
    }
}