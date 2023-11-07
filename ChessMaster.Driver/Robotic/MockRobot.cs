using ChessMaster.RobotDriver.State;
using System.Numerics;

namespace ChessMaster.RobotDriver.Robotic
{
    public class MockRobot /*: IRobot*/
    {
        private bool running = false;
        private Vector3 position = new Vector3(0f, 0f, 0f);
        private float originX = -490f, originY = -820f, originZ = -200f;
        public Vector3 Limits { get { return new Vector3(-originX, -originY, -originZ); } }


        public MockRobot()
        {
        }

        public async Task InitializeAsync()
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

        public async Task MoveAsync(float x, float y, float z)
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

        public async Task OpenGripAsync()
        {
            await Task.Delay(50);
        }

        public async Task CloseGrip()
        {
            await Task.Delay(50);
        }

        public async Task<RobotState> GetState()
        {
            await Task.Delay(50);
            return new RobotState(MovementState.Idle, position.X, position.Y, position.Z);
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
}
