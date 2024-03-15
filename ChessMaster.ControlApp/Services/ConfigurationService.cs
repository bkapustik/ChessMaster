using ChessMaster.ChessDriver;
using ChessMaster.ControlApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace ChessMaster.ControlApp.Services;
public class ConfigurationService
{
    private static readonly Lazy<ConfigurationService> instance =
        new Lazy<ConfigurationService>(() => new ConfigurationService());

    private readonly ChessRunner chessRunner;

    public static ConfigurationService Instance => instance.Value;

    private ConfigurationService()
    {
        chessRunner = ChessRunner.Instance;
        A1Corner = new();
        H8Corner = new();
    }

    public bool IsRobotAtDesiredPosition() => chessRunner.IsRobotAtDesiredPosition(RobotDesiredPosition);

    public List<string> AcceptedFileTypes { get; set; } = new List<string>();
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
    public Vector3 ControlDesiredPosition(string buttonName, long ticksHeld)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(buttonName);
        if (IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(RobotDesiredPosition.X + direction.X * ticksHeld,
            RobotDesiredPosition.Y + direction.Y * ticksHeld,
            RobotDesiredPosition.Z);
    }
    public Vector3 ControlDesiredPosition(VirtualKey key, long ticksHeld)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(key);
        if (IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(RobotDesiredPosition.X + direction.X * ticksHeld,
            RobotDesiredPosition.Y + direction.Y * ticksHeld,
            RobotDesiredPosition.Z);
    }

    public void GoToA1() => RobotDesiredPosition = new Vector3(A1Corner.Position, chessRunner.GetConfigurationHeight());
    public void GoToH8() => RobotDesiredPosition = new Vector3(H8Corner.Position, chessRunner.GetConfigurationHeight());

    public void GoToDesiredPosition()
    {
        if (!IsRobotAtDesiredPosition())
        {
            Task.Run(() => chessRunner.ConfigurationMove(RobotDesiredPosition));
        }
    }

    public bool IsSpedUp { get; set; }
    public CornerPosition A1Corner { get; set; }
    public CornerPosition H8Corner { get; set; }

    public Vector3 RobotDesiredPosition { get; set; }

    public bool AreCornersLocked() => A1Corner.Locked && H8Corner.Locked;
}

