using ChessMaster.Robot.Robot;
using ChessMaster.Robot.State;
using ChessMaster.Space.Coordinations;
using System.Numerics;

namespace ChessMaster.Space.RobotSpace
{
    public class RobotSpace
    {
        private readonly Space space;
        private readonly IRobot robot;

        private MoveableEntity? currentlyHeldEntity;

        public RobotSpace(Space space, IRobot robot)
        {   
            this.space = space;
            this.robot = robot;
        }

        private void TakeEntityFromPosition(SpaceVector position)
        {
            var entity = space.SubSpaces[position.X, position.Y].Entity;
            var entityCenter = entity!.Get2DCenter();
            var aboveEntityPosition = new Vector3(entityCenter.X, entityCenter.Y, entity.Height + 10);

            robot.Move(aboveEntityPosition);

            robot.OpenGrip();

            robot.Move(entity.GetHoldingPointVector());

            robot.CloseGrip();

            currentlyHeldEntity = entity;

            space.SubSpaces[position.X, position.Y].Entity = null;
        }

        private void MoveEntityToPosition(SpaceVector targetPosition)
        {
            robot.Move(space.SubSpaces[targetPosition.X, targetPosition.Y].GetCenter());

            robot.OpenGrip();

            robot.MoveZ(currentlyHeldEntity!.Height + 10);

            space.SubSpaces[targetPosition.X, targetPosition.Y].Entity = currentlyHeldEntity;

            currentlyHeldEntity = null;

            robot.CloseGrip();
        }

        public void MoveEntityFromSourceToTarget(SpaceVector source, SpaceVector target)
        {
            TakeEntityFromPosition(source);

            MoveEntityToPosition(target);
        }
    }
}
