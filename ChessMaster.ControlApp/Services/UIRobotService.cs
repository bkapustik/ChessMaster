using ChessMaster.ChessDriver;
using ChessMaster.ChessDriver.Models;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;

namespace ChessMaster.ControlApp.Services;

/// <summary>
/// Singleton game state service
/// </summary>
public class UIRobotService
{
    private static readonly Lazy<UIRobotService> instance =
        new Lazy<UIRobotService>(() => new UIRobotService());

    public static UIRobotService Instance => instance.Value;

    private readonly ChessRunner chessRunner;

    public UIGameState UIGameState { get; set; }

    public ObservableCollection<string> GameMessages { get; set; }
    public DispatcherTimer Timer { get; private set; }
    public bool MessagesInitialized { get; set; } = false;
    public bool CanMove()
    {
        var state = chessRunner.GetRobotState();

        UIGameState.MovementState = state.MovementState;
        UIGameState.RobotState = state.RobotResponse;

        return (UIGameState.RobotState == RobotResponse.Ok || UIGameState.RobotState == RobotResponse.Initialized) &&
            (UIGameState.MovementState == MovementState.Idle || UIGameState.MovementState == MovementState.Holding);
    }

    public void MoveToDesiredPosition()
    {

    }

    private UIRobotService()
    {
        UIGameState = new UIGameState();
        GameMessages = new ObservableCollection<string>();
        Timer = new DispatcherTimer();
        chessRunner = ChessRunner.Instance;
    }
}