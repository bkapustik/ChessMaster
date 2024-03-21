using ChessMaster.ChessDriver;
using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ControlApp.Helpers;
using ChessMaster.ControlApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class SelectStrategyPage : Page
{
    private MainWindow mainWindow;
    private List<ChessStrategyFacade> Strategies = new();

    private IChessRunner chessRunner;
    private IUIRobotService robotService;

    public SelectStrategyPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        chessRunner = App.Services.GetRequiredService<IChessRunner>();
        robotService = App.Services.GetRequiredService<IUIRobotService>();

        var strategies = chessRunner.GetStrategies();
        Strategies.AddRange(strategies);

        StrategyComboBox.SelectedIndex = 0;

        var controlFactory = new ControlFactory(mainWindow);
        mainWindow.AddMenuButton(controlFactory.CreateBackToConfigurationButton());
        if (robotService.UIGameState.GameState == ChessDriver.Events.GameState.InProgress)
        {
            mainWindow.AddMenuButton(controlFactory.CreateContinueInGameButton());
        }
    }

    private void StrategySelectedButton(object sender, RoutedEventArgs e)
    {
        var selectedStrategy = (ChessStrategyFacade)StrategyComboBox.SelectedValue;

        mainWindow.PickStrategy(selectedStrategy);
    }
}
