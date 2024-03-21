using ChessMaster.ChessDriver;
using ChessMaster.ControlApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.System;

namespace ChessMaster.ControlApp.Services;
public class ConfigurationService : IConfigurationService
{
    private readonly IChessRunner chessRunner;

    public ConfigurationService()
    {
        A1Corner = new();
        H8Corner = new();

        chessRunner = App.Services.GetRequiredService<IChessRunner>();
    }

    public bool IsRobotAtDesiredPosition() => chessRunner.IsRobotAtDesiredPosition(RobotDesiredPosition);

    public List<string> AcceptedFileTypes { get; set; } = new List<string>();
  
   
    public Vector3 ControlDesiredPosition(string buttonName, long ticksHeld)
    {
        ticksHeld /= 10;

        var direction = IConfigurationService.GetDirectionVector(buttonName);
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

        var direction = IConfigurationService.GetDirectionVector(key);
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

