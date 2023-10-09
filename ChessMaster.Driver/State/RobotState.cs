using System.Numerics;

namespace ChessMaster.Robot.State
{
    public struct RobotState
    {
        public readonly MovementState MovementState;
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public Vector3 Position { get { return new Vector3(x, y, z); } }

        public RobotState(MovementState state, float x, float y, float z)
        {
            MovementState = state;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
