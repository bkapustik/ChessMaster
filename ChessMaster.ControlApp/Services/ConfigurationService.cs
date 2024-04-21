using ChessMaster.ChessDriver.Services;
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

    private float Speed { get; set; } = DEFAULT_SPEED;

    private const float SLOW_SPEED = 1f;
    private const float DEFAULT_SPEED = 10f;
    private const float FAST_SPEED = 100f;

    public ConfigurationService()
    {
        A1Corner = new();
        H8Corner = new();

        chessRunner = App.Services.GetRequiredService<IChessRunner>();
    }
    public string FilePickerTitleText { get; set; }

    public bool IsRobotAtDesiredPosition() => chessRunner.IsRobotAtDesiredPosition(RobotDesiredPosition);

    public List<string> AcceptedFileTypes { get; set; } = new List<string>();
    public void IncrementSpeed()
    {
        Speed = Speed == DEFAULT_SPEED ? FAST_SPEED : Speed == FAST_SPEED ? SLOW_SPEED : DEFAULT_SPEED;
    }
   
    public Vector3 ControlDesiredPosition(string buttonName)
    {
        var direction = IConfigurationService.GetDirectionVector(buttonName);

        return new Vector3(RobotDesiredPosition.X + direction.X * Speed,
            RobotDesiredPosition.Y + direction.Y * Speed,
            RobotDesiredPosition.Z);
    }

    public Vector3 ControlDesiredPosition(VirtualKey key)
    {
        var direction = IConfigurationService.GetDirectionVector(key);

        return new Vector3(RobotDesiredPosition.X + direction.X * Speed,
            RobotDesiredPosition.Y + direction.Y * Speed,
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

    public CornerPosition A1Corner { get; set; }
    public CornerPosition H8Corner { get; set; }

    public Vector3 RobotDesiredPosition { get; set; }

    public bool AreCornersLocked() => A1Corner.Locked && H8Corner.Locked;
}

