using ChessMaster.ControlApp.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.System;

namespace ChessMaster.ControlApp.Services;
public interface IConfigurationService
{
    const float TICK_SPEED = 100;
    List<string> AcceptedFileTypes { get; set; }
    string FilePickerTitleText { get; set; }
    void IncrementSpeed();
    CornerPosition A1Corner { get; set; }
    CornerPosition H8Corner { get; set; }
    Vector3 RobotDesiredPosition { get; set; }
    Vector3 ControlDesiredPosition(string buttonName);
    Vector3 ControlDesiredPosition(VirtualKey key);
    void GoToA1();
    void GoToH8();
    void GoToDesiredPosition();
    bool IsRobotAtDesiredPosition();
    bool AreCornersLocked();

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
