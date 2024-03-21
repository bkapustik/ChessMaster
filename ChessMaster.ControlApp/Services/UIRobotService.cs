using ChessMaster.ChessDriver;
using ChessMaster.ChessDriver.Models;
using ChessMaster.RobotDriver.Robotic;
using ChessMaster.RobotDriver.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;

namespace ChessMaster.ControlApp.Services;

/// <summary>
/// Singleton game state service
/// </summary>
public class UIRobotService : IUIRobotService
{
    private readonly ChessRunner chessRunner;

    public UIGameState UIGameState { get; set; }

    public ObservableCollection<string> GameMessages { get; set; }
    public DispatcherTimer Timer { get; private set; }
    public bool MessagesInitialized { get; set; } = false;

    public UIRobotService()
    {
        UIGameState = new UIGameState();
        GameMessages = new ObservableCollection<string>();
        Timer = new DispatcherTimer();

        chessRunner = App.Services.GetRequiredService<ChessRunner>();
    }

    public bool CanMove()
    {
        var state = chessRunner.GetRobotState();

        UIGameState.MovementState = state.MovementState;
        UIGameState.RobotState = state.RobotResponse;

        return (UIGameState.RobotState == RobotResponse.Ok || UIGameState.RobotState == RobotResponse.Initialized) &&
            (UIGameState.MovementState == MovementState.Idle || UIGameState.MovementState == MovementState.Holding);
    }
}