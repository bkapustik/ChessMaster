using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic
{
    public class MockRobot : IRobot
    {
        private bool running = false;
        private Vector3 position = new Vector3(0f, 0f, 0f);
        private float originX = -490f, originY = -820f, originZ = -200f;
        public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }

        public void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
        {

        }

        public MockRobot()
        {

        }

        public async Task Initialize()
        {
            await Task.Delay(2500);  
        }

        public void Reset()
        {
            Thread.Sleep(1000);
        }

        public void Home()
        {
            Thread.Sleep(5000);
        }

        public void Move(float x, float y, float z)
        {
            Thread.Sleep(500);
        }

        public void Move(Vector3 targetPosition)
        {
            Thread.Sleep(500);
        }

        public void MoveXY(float x, float y)
        {
            Thread.Sleep(500);
        }

        public void MoveX(float x)
        {
            Thread.Sleep(500);
        }

        public void MoveY(float y)
        {
            Thread.Sleep(500);
        }

        public void MoveZ(float z)
        {
            Thread.Sleep(500);
        }

        public void OpenGrip()
        {
            Thread.Sleep(200);
        }

        public void CloseGrip()
        {
            Thread.Sleep(500);
        }

        public async Task<RobotState> GetState()
        {
            await Task.Delay(50);
            return new RobotState(MovementState.Idle, position.X, position.Y, position.Z);
        }

        public void Pause()
        {
            Thread.Sleep(500);
        }

        public void Resume()
        {
            Thread.Sleep(500);
        }

        public void Stop()
        {
            Thread.Sleep(500);
        }
    }
}
