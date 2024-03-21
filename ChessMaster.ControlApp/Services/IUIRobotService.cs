using ChessMaster.ChessDriver.Models;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace ChessMaster.ControlApp.Services;

public interface IUIRobotService
{
    UIGameState UIGameState { get; set; }
    ObservableCollection<string> GameMessages { get; set; }
    DispatcherTimer Timer { get; }
    bool MessagesInitialized { get; set; }
    bool CanMove();
}
