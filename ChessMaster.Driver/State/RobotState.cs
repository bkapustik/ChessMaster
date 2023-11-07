using System.Numerics;

namespace ChessMaster.RobotDriver.State
{
    public struct RobotState
    {
        public MovementState MovementState;
        public GripState GripState;
        public float x;
        public float y;
        public float z;

        public Vector3 Position { get { return new Vector3(x, y, z); } }

        public RobotState(MovementState state, float x, float y, float z)
        {
            MovementState = state;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public RobotState(MovementState state, Vector3 position)
        { 
            MovementState = state;
            x = position.X;
            y = position.Y;
            z = position.Z;
        }

        public RobotState(MovementState state)
        {
            MovementState = state;    
        }

        public RobotState()
        { 
            
        }
    }
}
