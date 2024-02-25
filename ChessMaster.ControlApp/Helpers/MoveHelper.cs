using ChessMaster.ChessDriver.Models;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using System.Numerics;
using Windows.System;

namespace ChessMaster.ControlApp.Helpers;

public class MoveHelper
{
    public static bool CanMove(RobotResponse robotState, MovementState movementState)
    {
        return (robotState == RobotResponse.Ok || robotState == RobotResponse.Initialized) &&
            (movementState == MovementState.Idle || movementState == MovementState.Holding);
    }

    public static Vector3 ChangeDesiredPosition(VirtualKey key, long ticksHeld, UIGameState state)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(key);
        if (state.IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(state.DesiredPosition.X + (direction.X * ticksHeld),
            state.DesiredPosition.Y + (direction.Y * ticksHeld),
            state.DesiredPosition.Z);
    }

    public static Vector3 ChangeDesiredPosition(string buttonName, long ticksHeld, UIGameState state)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(buttonName);
        if (state.IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(state.DesiredPosition.X + (direction.X * ticksHeld),
            state.DesiredPosition.Y + (direction.Y * ticksHeld),
            state.DesiredPosition.Z);
    }

    public static Vector2 GetDirectionVector(string buttonName)
    {
        switch (buttonName)
        {
            case "Up":
                return new Vector2(0, 1);
            case "Down":
                return new Vector2(0, -1);
            case "Left":
                return new Vector2(-1, 0);
            case "Right":
                return new Vector2(1, 0);
            default:
                return new Vector2(0, 0);
        }
    }

    public static Vector2 GetDirectionVector(VirtualKey key)
    {
        switch (key)
        {
            case VirtualKey.Up:
                return new Vector2(0, 1);
            case VirtualKey.Down:
                return new Vector2(0, -1);
            case VirtualKey.Left:
                return new Vector2(-1, 0);
            case VirtualKey.Right:
                return new Vector2(1, 0);
            default:
                return new Vector2(0, 0);
        }
    }
}
