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
using ChessMaster.ControlApp.Helpers;
using System.ComponentModel;
using ChessMaster.RobotDriver.Events;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class ConfigurationPage : Page, INotifyPropertyChanged
{
    private MainWindow mainWindow;
    public List<Button> AllButtons { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private string a1Value;
    private string h8Value;

    private const string LOCK_A1_BUTTON_TEXT = "Lock A1";
    private const string UNLOCK_A1_BUTTON_TEXT = "Unlock A1";
    private const string LOCK_H8_BUTTON_TEXT = "Lock H8";
    private const string UNLOCK_H8_BUTTON_TEXT = "Unlock H8";

    public string A1Value
    {
        get { return a1Value; }
        set
        {
            a1Value = value;
            OnPropertyChanged(nameof(A1Value));
        }
    }
    public string H8Value
    {
        get { return h8Value; }
        set
        {
            h8Value = value;
            OnPropertyChanged(nameof(H8Value));
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

        if (mainWindow.UIGameState.IsA1Locked)
        {
            LockA1.Content = UNLOCK_A1_BUTTON_TEXT;
        }
        else
        {
            LockA1.Content = LOCK_A1_BUTTON_TEXT;
        }

        if (mainWindow.UIGameState.IsH8Locked)
        {
            LockH8.Content = UNLOCK_H8_BUTTON_TEXT;
        }
        else
        {
            LockH8.Content = LOCK_H8_BUTTON_TEXT;
        }


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

        var holdableUpButton = new HoldableMoveButton(Up, mainWindow.UIGameState);
        var holdableDownButton = new HoldableMoveButton(Down, mainWindow.UIGameState);
        var holdableLeftButton = new HoldableMoveButton(Left, mainWindow.UIGameState);
        var holdableRightButton = new HoldableMoveButton(Right, mainWindow.UIGameState);

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

        H8Value = mainWindow.UIGameState.GetH8String();
        A1Value = mainWindow.UIGameState.GetA1String();

        var controlFactory = new ControlFactory(mainWindow);

        if (mainWindow.UIGameState.GameState == ChessDriver.Events.GameState.InProgress)
        {
            mainWindow.AddMenuButton(controlFactory.CreateContinueInGameButton());
        }

        Task.Run(mainWindow.ChessRunner.Initialize);
    }

    private void RequireHoming()
    {
        mainWindow.UIGameState.RobotState = RobotResponse.HomingRequired;

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
        mainWindow.UIGameState.IsSpedUp = !mainWindow.UIGameState.IsSpedUp;
    }

    private void LockCorner(object sender, RoutedEventArgs e)
    {
        if (mainWindow.UIGameState.RobotState == RobotResponse.HomingRequired)
        {
            return;
        }

        var button = sender as Button;
        if (button.Name == "LockA1")
        {
            if (mainWindow.UIGameState.IsA1Locked)
            {
                mainWindow.UIGameState.IsA1Locked = false;
                mainWindow.DispatcherQueue.TryEnqueue(() => { button.Content = LOCK_A1_BUTTON_TEXT; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = mainWindow.ChessRunner.robot.GetState();
                    if (mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.UIGameState.DesiredPosition))
                    {
                        mainWindow.UIGameState.IsA1Locked = true;
                        mainWindow.UIGameState.A1Position = mainWindow.UIGameState.DesiredPosition.ToVector2();
                        mainWindow.DispatcherQueue.TryEnqueue(() => { 
                            button.Content = UNLOCK_A1_BUTTON_TEXT;
                            A1Value = mainWindow.UIGameState.GetA1String();
                        });
                        
                    }
                });
            }
        }
        else if (button.Name == "LockH8")
        {
            if (mainWindow.UIGameState.IsH8Locked)
            {
                mainWindow.UIGameState.IsH8Locked = false;
                mainWindow.DispatcherQueue.TryEnqueue(() => { button.Content = LOCK_H8_BUTTON_TEXT; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = mainWindow.ChessRunner.robot.GetState();
                    if (mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.UIGameState.DesiredPosition))
                    {
                        mainWindow.UIGameState.IsH8Locked = true;
                        mainWindow.UIGameState.H8Position = mainWindow.UIGameState.DesiredPosition.ToVector2();
                        mainWindow.DispatcherQueue.TryEnqueue(() => {
                            button.Content = UNLOCK_H8_BUTTON_TEXT;
                            H8Value = mainWindow.UIGameState.GetH8String();
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
        var state = mainWindow.ChessRunner.robot.GetState();

        mainWindow.UIGameState.MovementState = state.MovementState;
        mainWindow.UIGameState.RobotState = state.RobotResponse;

        if (!CanMove())
        {
            return;
        }

        bool atDesired = mainWindow.ChessRunner.robot.IsAtDesired(mainWindow.UIGameState.DesiredPosition);

        if (!atDesired)
        {
            Task.Run(() => mainWindow.ChessRunner.robot.Move(mainWindow.UIGameState.DesiredPosition));
        }
    }

    private bool CanMove()
    {
        return MoveHelper.CanMove(mainWindow.UIGameState.RobotState, mainWindow.UIGameState.MovementState);
    }
}
