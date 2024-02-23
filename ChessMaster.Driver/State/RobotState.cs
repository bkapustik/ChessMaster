using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.RobotDriver.State
{
    public struct RobotState
    {
        public RobotResponse RobotResponse;
        public MovementState MovementState;
        public GripState GripState;
        public float x;
        public float y;
        public float z;

        public Vector3 Position { get { return new Vector3(x, y, z); } }
        public RobotState(MovementState state, RobotResponse response, Vector3 position)
        {
            RobotResponse = response;
            MovementState = state;
            x = position.X;
            y = position.Y;
            z = position.Z;
        }

        public RobotState(MovementState state, RobotResponse response, float x, float y, float z)
        {
            RobotResponse = response;
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

        public void Update(Vector3 position)
        { 
            x = position.X; 
            y = position.Y; 
            z = position.Z;
        }
    }
}
