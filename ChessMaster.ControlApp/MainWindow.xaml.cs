using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ChessMaster.ControlApp;

public enum ControlAppMode
{
    PortSelection,
    RobotPositionConfiguration
}

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private int windowWidth = 350;
    private int windowHeight = 250;

    private string SelectedPort;
    private DispatcherTimer timer;
    private ControlAppMode appMode = ControlAppMode.PortSelection;
    private ChessRunner chessRunner;
    private Vector2 desiredPosition;
    private Vector2 a1Position;
    private Vector2 h8Position;
    private bool isA1Locked;
    private bool isH8Locked;
    private bool IsSpedUp;
    private bool IsConfigured;

    public MainWindow()
    {
        this.InitializeComponent();
        Initialize();

        timer = new DispatcherTimer();
    }

    private List<string> Ports = new()
    {
        "DUMMY"
    };

    private List<string> Strategies;

    private void PortSelectionLoaded(object sender, RoutedEventArgs e)
    {
        PortComboBox.SelectedIndex = 0;
        var ports = SerialPort.GetPortNames();
        Ports.AddRange(ports);
    }

    private void CenterToScreen(IntPtr hWnd)
    {
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow is not null)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            if (displayArea is not null)
            {
                var CenteredPosition = appWindow.Position;
                CenteredPosition.X = ((displayArea.WorkArea.Width - appWindow.Size.Width) / 2);
                CenteredPosition.Y = ((displayArea.WorkArea.Height - appWindow.Size.Height) / 2);
                appWindow.Move(CenteredPosition);
            }
        }
    }

    private void Initialize()
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));
        CenterToScreen(windowHandle);
    }

    private void StrategySelectedButton(object sender, RoutedEventArgs e)
    { 
        
    }

    private void PortSelectedButton(object sender, RoutedEventArgs e)
    {
        SelectedPort = (string)PortComboBox.SelectedValue;

        chessRunner = new ChessRunner(new MockRobot());
        Task.Run(chessRunner.InitializeMock);

        chessRunner.robot.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        {
            var position = chessRunner.robot.GetState().Result.Position;
            desiredPosition = new Vector2(position.X, position.Y);
        }));

        StrategyComboBox.SelectedIndex = 0;
        Strategies = chessRunner.GetStrategies();

        appMode = ControlAppMode.RobotPositionConfiguration;
        PortGrid.Visibility = Visibility.Collapsed;

        //TODO !!!!!!!!!!!!!!! ChessRunner = new ChessRunner(SelectedPort); 
        ConfigurationGrid.Visibility = Visibility.Visible;

        timer.Tick += DispatcherTimerTick;
        timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        timer.Start();


    }

    private void SpeedUpButton(object sender, RoutedEventArgs e)
    {
        IsSpedUp = !IsSpedUp;
    }

    private void MoveButton(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;

        var direction = GetDirectionVector(button.Name);
        if (IsSpedUp)
        {
            direction = direction * 10;
        }
        desiredPosition += direction;
    }

    private void DispatcherTimerTick(object sender, object args)
    {
        var desired = desiredPosition;
        Task.Run(() =>
        {
            bool atDesired = IsAtDesired(desired);

            if (!atDesired)
            {
                chessRunner.robot.Move(desired);
            }
        });
    }

    private Vector2 GetDirectionVector(string buttonName)
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

    private void LockCorner(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button.Name == "LockA1")
        {
            if (isA1Locked)
            {
                isA1Locked = false;
                DispatcherQueue.TryEnqueue(() => { button.Content = "Lock A1"; });
            }
            else
            {
                Task.Run(() =>
                {
                    if (IsAtDesired(desiredPosition))
                    {
                        isA1Locked = true;
                        a1Position = desiredPosition;
                        DispatcherQueue.TryEnqueue(() => { button.Content = "Unlock A1"; });
                    }
                });
            }
        }
        else if (button.Name == "LockH8")
        {
            if (isH8Locked)
            {
                isH8Locked = false;
                DispatcherQueue.TryEnqueue(() => { button.Content = "Lock H8"; });
            }
            else
            {
                Task.Run(() =>
                {
                    if (IsAtDesired(desiredPosition))
                    {
                        isH8Locked = true;
                        h8Position = desiredPosition;
                        DispatcherQueue.TryEnqueue(() => { button.Content = "Unlock H8"; });
                    }
                });
            }
        }
    }

    private bool IsAtDesired(Vector2 desired)
    {
        var state = chessRunner.robot.GetState().Result;
        float dx = desired.X - state.Position.X;
        float dy = desired.Y - state.Position.Y;

        return Math.Abs(dx) <= 0.5 && Math.Abs(dy) <= 0.5;
    }

    private void ConfirmConfiguration(object sender, RoutedEventArgs e)
    {
        if (!isA1Locked || !isH8Locked)
        {
            return;
        }

        IsConfigured = true;

        ConfigurationGrid.Visibility = Visibility.Collapsed;
        StrategyPickGrid.Visibility = Visibility.Visible;
    }

    private void PickPawn(object sender, RoutedEventArgs e)
    {
        Task.Run(chessRunner.robot.ConfigurationPickPawn);
    }

    private void ReleasePawn(object sender, RoutedEventArgs e)
    {
       Task.Run(chessRunner.robot.ConfigurationReleasePawn);
    }
}