using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Robot.State
{
    public enum MovementState
    {
        Idle,
        Running,
        Holding,
        Unknown
    }

    public static class MovementStateExtensions
    {
        public static string ToString(this MovementState state)
        {
            switch(state) 
            {
                case MovementState.Idle:
                    return "Idle";
                case MovementState.Running:
                    return "Run";
                case MovementState.Holding:
                    return "Hold";
                default:
                    return "";
            }
        }

        public static MovementState ToMovementState(this string state)
        {
            switch (state)
            {
                case "Idle":
                    return MovementState.Idle;
                case "Run":
                    return MovementState.Running;
                case "Hold":
                    return MovementState.Holding;
                default:
                    return MovementState.Unknown;
            }
        }
    }
}
