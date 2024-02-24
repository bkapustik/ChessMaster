using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Data;
using Windows.System;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using ChessMaster.Space.Coordinations;
using System.Numerics;
using ChessMaster.RobotDriver.State;
using ChessMaster.ControlApp.Helpers;
using System.ComponentModel;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class ConfigurationPage : Page, INotifyPropertyChanged
{
    private MainWindow mainWindow;

    public List<Button> AllButtons { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private string a1Value;
    private string h8Value;

    public string A1Value
    {
        get { return a1Value; }
        set
        {
            if (a1Value != value)
            { 
                a1Value = value;
                OnPropertyChanged(nameof(A1Value));
            }
        }
    }
    public string H8Value
    {
        get { return h8Value; }
        set
        {
            if (h8Value != value)
            { 
                h8Value = value;
                OnPropertyChanged(nameof(H8Value));
            }
        }
    }

    public ConfigurationPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;

        AllButtons = new List<Button>()
        {
            Up,
            Down,
            Left,
            Right,
            Speed,
            LockA1,
            LockH8,
            PickPawnButton,
            ReleasePawnButton,
            ConfirmButton,
            HomeButton
        };

        mainWindow.Timer.Tick += MovementControl;

        var holdableUpButton = new HoldableMoveButton(Up, mainWindow.SetupState);
        var holdableDownButton = new HoldableMoveButton(Down, mainWindow.SetupState);
        var holdableLeftButton = new HoldableMoveButton(Left, mainWindow.SetupState);
        var holdableRightButton = new HoldableMoveButton(Right, mainWindow.SetupState);

        mainWindow.RegisterKeyboardControl(VirtualKey.Up);
        mainWindow.RegisterKeyboardControl(VirtualKey.Down);
        mainWindow.RegisterKeyboardControl(VirtualKey.Left);
        mainWindow.RegisterKeyboardControl(VirtualKey.Right);

        mainWindow.ChessRunner.robot.Robot.HomingRequired += (object o, RobotEventArgs e) => RequireHoming();

        var a1Label = new TextBlock
        {
            Text = "A1:"
        };
        var a1Value = new TextBlock();
        a1Value.SetBinding(TextBlock.TextProperty, new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(A1Value)),
            Mode = BindingMode.OneWay
        });

        var h8Label = new TextBlock
        {
            Text = "H8:"
        };
        var h8Value = new TextBlock();
        h8Value.SetBinding(TextBlock.TextProperty, new Binding
        {
            Source = this,
            Path = new PropertyPath(nameof(H8Value)),
            Mode = BindingMode.OneWay
        });

        mainWindow.AddLeftTabInfo(a1Label, a1Value);
        mainWindow.AddLeftTabInfo(h8Label, h8Value);

        Task.Run(mainWindow.ChessRunner.Initialize);
    }

    private void RequireHoming()
    {
        mainWindow.SetupState.RobotState = RobotResponse.HomingRequired;

        foreach (var button in AllButtons)
        {
            if (button != HomeButton)
            {
                DispatcherQueue.TryEnqueue(() => button.IsEnabled = false);
            }
        }
    }

    private void SpeedUpButton(object sender, RoutedEventArgs e)
    {
        mainWindow.SetupState.IsSpedUp = !mainWindow.SetupState.IsSpedUp;
    }

    private void LockCorner(object sender, RoutedEventArgs e)
    {
        if (mainWindow.SetupState.RobotState == RobotResponse.HomingRequired)
        {
            return;
        }

        var button = sender as Button;
        if (button.Name == "LockA1")
        {
            if (mainWindow.SetupState.IsA1Locked)
            {
                mainWindow.SetupState.IsA1Locked = false;
                mainWindow.DispatcherQueue.TryEnqueue(() => { button.Content = "Lock A1"; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = mainWindow.ChessRunner.robot.GetState();
                    if (mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.SetupState.DesiredPosition, state))
                    {
                        mainWindow.SetupState.IsA1Locked = true;
                        mainWindow.SetupState.A1Position = mainWindow.SetupState.DesiredPosition.ToVector2();
                        mainWindow.DispatcherQueue.TryEnqueue(() => { 
                            button.Content = "Unlock A1";
                            A1Value = mainWindow.SetupState.A1Position.ToString();
                        });
                        
                    }
                });
            }
        }
        else if (button.Name == "LockH8")
        {
            if (mainWindow.SetupState.IsH8Locked)
            {
                mainWindow.SetupState.IsH8Locked = false;
                mainWindow.DispatcherQueue.TryEnqueue(() => { button.Content = "Lock H8"; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = mainWindow.ChessRunner.robot.GetState();
                    if (mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.SetupState.DesiredPosition, state))
                    {
                        mainWindow.SetupState.IsH8Locked = true;
                        mainWindow.SetupState.H8Position = mainWindow.SetupState.DesiredPosition.ToVector2();
                        mainWindow.DispatcherQueue.TryEnqueue(() => {
                            button.Content = "Unlock H8";
                            H8Value = mainWindow.SetupState.H8Position.ToString();
                        });
                    }
                });
            }
        }
    }

    private void ConfirmConfiguration(object sender, RoutedEventArgs e)
    {
        mainWindow.ConfirmConfiguration();
    }

    private void PickPawn(object sender, RoutedEventArgs e)
    {
        Task.Run(mainWindow.ChessRunner.robot.ConfigurationPickPawn);
    }

    private void ReleasePawn(object sender, RoutedEventArgs e)
    {
        Task.Run(mainWindow.ChessRunner.robot.ConfigurationReleasePawn);
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void Home(object sender, RoutedEventArgs e)
    {
        Task.Run(mainWindow.ChessRunner.robot.Home);

        foreach (var button in AllButtons)
        {
            if (button != HomeButton)
            {
                mainWindow.DispatcherQueue.TryEnqueue(() => button.IsEnabled = true);
            }
        }
    }

    private void MovementControl(object sender, object args)
    {
        if (!CanMove())
        {
            return;
        }

        var state = mainWindow.ChessRunner.robot.GetState();

        if (state.MovementState == MovementState.Unknown)
        {
            return;
        }

        bool atDesired = mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.SetupState.DesiredPosition, state);

        if (!atDesired)
        {
            Task.Run(() => mainWindow.ChessRunner.robot.Move(mainWindow.SetupState.DesiredPosition));
        }
    }

    private bool CanMove()
    {
        return MoveHelper.CanMove(mainWindow.SetupState.RobotState);
    }
}
