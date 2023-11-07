using ChessMaster.RobotDriver.Robotic;
using ChessMaster.Space.Coordinations;
using System.Diagnostics.Tracing;
using System.Numerics;

namespace ChessMaster.Space.RobotSpace
{
    public class RobotSpace
    {
        protected Space[] space;
        protected IRobot robot;

        private MoveableEntity? currentlyHeldEntity;

        public void Initialize(Space[] space, IRobot robot)
        { 
            this.space = space; 
            this.robot = robot;
        }

        protected void MoveEntityFromSourceToTarget(SpacePosition source, SpacePosition target, int sourceSpaceIndex = 0, int targetSpaceIndex = 0)
        {
            TakeEntityFromPosition(source, sourceSpaceIndex);

            MoveEntityToPosition(target, targetSpaceIndex);
        }
        protected void SubscribeToCommandsCompletion(CommandsCompletedEvent e)
        {
            robot.SubscribeToCommandsCompletion(e);
        }
        private void TakeEntityFromPosition(SpacePosition position, int spaceIndex)
        {
            var entity = space[spaceIndex].SubSpaces[position.X,position.Y].Entity;
            var entityCenter = entity!.Get2DCenter();
            var aboveEntityPosition = new Vector3(entityCenter.X, entityCenter.Y, entity.Height + 10);

            robot.Move(aboveEntityPosition);

            robot.OpenGrip();

            robot.Move(entity.GetHoldingPointVector());

            robot.CloseGrip();

            currentlyHeldEntity = entity;

            space[spaceIndex].SubSpaces[position.X, position.Y].Entity = null;
        }
        private void MoveEntityToPosition(SpacePosition targetPosition, int spaceIndex)
        {
            robot.Move(space[spaceIndex].SubSpaces[targetPosition.X, targetPosition.Y].GetCenter());

            robot.OpenGrip();

            robot.MoveZ(currentlyHeldEntity!.Height + 10);

            space[spaceIndex].SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;

            currentlyHeldEntity = null;

            robot.CloseGrip();
        }
    }
}
