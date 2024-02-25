using ChessMaster.ChessDriver.ChessStrategy;
using ChessMaster.ControlApp.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class SelectStrategyPage : Page
{
    private MainWindow mainWindow;
    private List<ChessStrategyFacade> Strategies = new();

    public SelectStrategyPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;

        var strategies = mainWindow.ChessRunner.GetStrategies();
        Strategies.AddRange(strategies);

        StrategyComboBox.SelectedIndex = 0;

        var controlFactory = new ControlFactory(mainWindow);
        mainWindow.AddMenuButton(controlFactory.CreateBackToConfigurationButton());
        if (mainWindow.UIGameState.GameState == ChessDriver.Events.GameState.InProgress)
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
