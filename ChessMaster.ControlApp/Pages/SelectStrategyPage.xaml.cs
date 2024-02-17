using ChessMaster.ChessDriver;
using ChessMaster.ChessDriver.ChessStrategy;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
    }

    private void StrategySelectedButton(object sender, RoutedEventArgs e)
    {
        var selectedStrategy = (ChessStrategyFacade)StrategyComboBox.SelectedValue;

        mainWindow.PickStrategy(selectedStrategy);
    }
}
