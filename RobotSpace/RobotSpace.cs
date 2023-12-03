using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System.Diagnostics.Tracing;
using System.Numerics;
using ChessMaster.RobotDriver.State;

namespace ChessMaster.Space.RobotSpace
{
    public class RobotSpace
    {
        protected Space[] spaces;
        protected IRobot robot;

        private MoveableEntity? currentlyHeldEntity;
        private RobotState currentRobotState;
        private int currentSpaceIndex;
        private SpacePosition currentPosition;

        public void Initialize(Space[] spaces, IRobot robot)
        { 
            this.spaces = spaces; 
            this.robot = robot;
            robot.Initialize().Wait();
        }

        protected void MoveEntityFromSourceToTarget(SpacePosition source, SpacePosition target, int sourceSpaceIndex = 0, int targetSpaceIndex = 0)
        {
            TakeEntityFromPosition(source, sourceSpaceIndex);

            MoveToCarryingPosition(target, targetSpaceIndex);

            MoveEntityToPosition(target, targetSpaceIndex);
        }
        public void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
        {
            robot.SubscribeToCommandsCompletion(e);
        }

        private void UpdateCurrentState(SpacePosition position, int spaceIndex)
        { 
            currentSpaceIndex = spaceIndex;
            currentPosition = position;
        }
        
        public async Task<RobotState> GetState() => await robot.GetState();

        private void TakeEntityFromPosition(SpacePosition position, int spaceIndex)
        {
            var entity = spaces[spaceIndex].SubSpaces[position.X,position.Y].Entity;

            var moves = GetBestTrajectory(position, spaceIndex);

            foreach (var move in moves)
            { 
                robot.Move(move);
            }

            robot.OpenGrip();

            robot.Move(entity!.GetHoldingPointVector());

            robot.CloseGrip();

            currentlyHeldEntity = entity;

            spaces[spaceIndex].SubSpaces[position.X, position.Y].Entity = null;

            UpdateCurrentState(position, spaceIndex);
        }
        
        private void MoveEntityToPosition(SpacePosition targetPosition, int spaceIndex)
        {
            spaces[spaceIndex].SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;

            currentlyHeldEntity = null;

            var moves = GetBestTrajectory(targetPosition, spaceIndex);

            foreach (var move in moves)
            {
                robot.Move(move);
            }

            robot.CloseGrip();

            UpdateCurrentState(targetPosition, spaceIndex);
        }

        private void MoveToCarryingPosition(SpacePosition spacePosition, int currentSpaceIndex)
        {
            var carryingMove = GetBestCarryingPosition(spacePosition, currentSpaceIndex);
            robot.Move(carryingMove);

            UpdateCurrentState(spacePosition, currentSpaceIndex);
        }

        private List<Vector3> GetBestTrajectory(SpacePosition targetPosition, int targetSpaceIndex)
        {
            var currentCoordinations = spaces[currentSpaceIndex].SubSpaces[currentPosition.X, currentPosition.Y].Center.Value;
            var targetCoordinations = spaces[targetSpaceIndex].SubSpaces[targetPosition.X, targetPosition.Y].Center.Value;

            //var intersectedSpaceIndices = new List<int>();

            //for (int i = 0; i < spaces.Length; i++)
            //{ 
            //    if ()
            //}



            return new List<Vector3>() { new Vector3(targetCoordinations.X, targetCoordinations.Y, 100) };
            //TODO implemenet
        }

        private Vector3 GetBestCarryingPosition(SpacePosition spacePosition, int currentSpaceIndex)
        {
            return new Vector3();
            //TODO implemenet
        }
    }
}
