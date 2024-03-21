using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Data;
using Windows.System;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using ChessMaster.ControlApp.Helpers;
using System.ComponentModel;
using ChessMaster.RobotDriver.Events;
using ChessMaster.ChessDriver;
using ChessMaster.ControlApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class ConfigurationPage : Page, INotifyPropertyChanged
{
    private MainWindow mainWindow;

    private IChessRunner chessRunner;
    private IUIRobotService robotService;
    private IConfigurationService configurationService;
    private DispatcherTimer timer;

    public List<Button> AllButtons { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;

    private string a1Value;
    private string h8Value;
    private bool pickingFigure = false;

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
        InitializeComponent();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        chessRunner.RobotStateEvents.HomingRequired -= RequireHoming;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        chessRunner = App.Services.GetRequiredService<IChessRunner>();
        robotService = App.Services.GetRequiredService<IUIRobotService>();
        configurationService = App.Services.GetRequiredService<IConfigurationService>();

        timer = new DispatcherTimer
        {
            Interval = new System.TimeSpan(0, 0, 0, 0, 100)
        };

        timer.Tick += UpdateLockButtons;
        timer.Start();

        SetCornerButtons();
        SaveButtonsWhichLockOnHomingRequired();

        if (robotService.UIGameState.ChessBoardInitialized)
        {
            ConfirmButton.Content = "Reconfigure";
        }

        StartMovementControl();
        SetMoveButtons();
        chessRunner.RobotStateEvents.HomingRequired += RequireHoming;

        SetCornerPositions();

        var controlFactory = new ControlFactory(mainWindow);

        if (robotService.UIGameState.GameState == ChessDriver.Events.GameState.InProgress)
        {
            mainWindow.AddMenuButton(controlFactory.CreateContinueInGameButton());
        }

        var goToA1Position = ControlFactory.CreateMenuButton("Go to A1");
        var goToH8Position = ControlFactory.CreateMenuButton("Go to H8");

        goToA1Position.Click += GoToA1Click;
        goToH8Position.Click += GoToH8Click;

        mainWindow.AddMenuButton(goToA1Position);
        mainWindow.AddMenuButton(goToH8Position);

        Task.Run(chessRunner.Initialize);
    }

    private void SetCornerButtons()
    {
        if (configurationService.A1Corner.Locked)
        {
            LockA1Button.Content = UNLOCK_A1_BUTTON_TEXT;
        }
        else
        {
            LockA1Button.Content = LOCK_A1_BUTTON_TEXT;
        }

        if (configurationService.H8Corner.Locked)
        {
            LockH8Button.Content = UNLOCK_H8_BUTTON_TEXT;
        }
        else
        {
            LockH8Button.Content = LOCK_H8_BUTTON_TEXT;
        }
    }

    private void SetCornerPositions()
    {
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

        H8Value = configurationService.H8Corner.GetPositionString();
        A1Value = configurationService.A1Corner.GetPositionString();
    }

    private void SetMoveButtons()
    {
        var holdableUpButton = new HoldableMoveButton(Up);
        var holdableDownButton = new HoldableMoveButton(Down);
        var holdableLeftButton = new HoldableMoveButton(Left);
        var holdableRightButton = new HoldableMoveButton(Right);

        mainWindow.RegisterKeyboardControl(VirtualKey.Up);
        mainWindow.RegisterKeyboardControl(VirtualKey.Down);
        mainWindow.RegisterKeyboardControl(VirtualKey.Left);
        mainWindow.RegisterKeyboardControl(VirtualKey.Right);
    }

    private void SaveButtonsWhichLockOnHomingRequired()
    {
        AllButtons = new List<Button>()
        {
            Up,
            Down,
            Left,
            Right,
            Speed,
            LockA1Button,
            LockH8Button,
            PickPawnButton,
            ReleasePawnButton,
            ConfirmButton,
            HomeButton
        };
    }

    private void RequireHoming(object o, RobotEventArgs e)
    {
        robotService.UIGameState.RobotState = RobotResponse.HomingRequired;

        foreach (var button in AllButtons)
        {
            if (button != HomeButton)
            {
                DispatcherQueue.TryEnqueue(() => button.IsEnabled = false);
            }
        }
    }

    private void StopMovementControl() => timer.Tick -= MovementControl;
    private void StartMovementControl() => timer.Tick += MovementControl;

    private void SpeedUpButton(object sender, RoutedEventArgs e)
    {
        configurationService.IsSpedUp = !configurationService.IsSpedUp;
    }

    private void LockA1Click(object sender, RoutedEventArgs e)
    {
        if (robotService.UIGameState.RobotState == RobotResponse.HomingRequired)
        {
            return;
        }
        configurationService.A1Corner.ChangeLock();
    }

    private void LockH8Click(object sender, RoutedEventArgs e)
    {
        if (robotService.UIGameState.RobotState == RobotResponse.HomingRequired)
        {
            return;
        }
        configurationService.H8Corner.ChangeLock();
    }

    private void UpdateLockButtons(object sender, object e)
    {
        if (configurationService.H8Corner.Locked)
        {
            mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                LockH8Button.Content = UNLOCK_H8_BUTTON_TEXT;
                H8Value = configurationService.H8Corner.GetPositionString();
            });
        }
        else
        {
            mainWindow.DispatcherQueue.TryEnqueue(() => { LockH8Button.Content = LOCK_H8_BUTTON_TEXT; });
        }

        if (configurationService.A1Corner.Locked)
        {
            mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                LockA1Button.Content = UNLOCK_A1_BUTTON_TEXT;
                A1Value = configurationService.A1Corner.GetPositionString();
            });
        }
        else
        {
            mainWindow.DispatcherQueue.TryEnqueue(() => { LockA1Button.Content = LOCK_A1_BUTTON_TEXT; });
        }
    }

    private void ConfirmConfiguration(object sender, RoutedEventArgs e)
    {
        mainWindow.ConfirmConfiguration();
    }

    private void PickPawn(object sender, RoutedEventArgs e)
    {
        if (pickingFigure)
        {
            return;
        }

        pickingFigure = true;

        bool canPickFigure = false;

        Task.Run(() =>
        {
            while (!canPickFigure)
            {
                canPickFigure = chessRunner.CanPickFigure();
            }

            StopMovementControl();

            chessRunner.RobotStateEvents.CommandsSucceeded += RestartMovementControl;
            chessRunner.RobotStateEvents.CommandsSucceeded += PickingFigureFinished;
            chessRunner.TryPickFigure(Chess.FigureType.Pawn);
        });
    }

    private void RestartMovementControl(object sender, RobotEventArgs e)
    { 
        configurationService.RobotDesiredPosition = e.RobotState.Position;

        StartMovementControl();
        chessRunner.RobotStateEvents.CommandsSucceeded -= RestartMovementControl;
    }

    private void PickingFigureFinished(object sender, RobotEventArgs e)
    {
        pickingFigure = false;
        chessRunner.RobotStateEvents.CommandsSucceeded -= PickingFigureFinished;
    }

    private void ReleasePawn(object sender, RoutedEventArgs e)
    {
        if (pickingFigure)
        {
            return;
        }

        pickingFigure = true;

        bool canPickFigure = false;

        Task.Run(() =>
        {
            while (!canPickFigure)
            {
                canPickFigure = chessRunner.CanPickFigure();
            }

            StopMovementControl();

            chessRunner.RobotStateEvents.CommandsSucceeded += RestartMovementControl;
            chessRunner.RobotStateEvents.CommandsSucceeded += PickingFigureFinished;
            chessRunner.TryReleaseFigure(Chess.FigureType.Pawn);
        });
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void Home(object sender, RoutedEventArgs e)
    {
        foreach (var button in AllButtons)
        {
            if (button != HomeButton)
            {
                mainWindow.DispatcherQueue.TryEnqueue(() => button.IsEnabled = true);
            }
        }

        StopMovementControl();
        chessRunner.RobotStateEvents.CommandsSucceeded += RestartMovementControl;

        Task.Run(chessRunner.Home);
    }

    private void MovementControl(object sender, object args)
    {
        if (!robotService.CanMove())
        {
            return;
        }

        configurationService.GoToDesiredPosition();
    }

    private void GoToA1Click(object sender, RoutedEventArgs e) => configurationService.GoToA1();

    private void GoToH8Click(object sender, RoutedEventArgs e) => configurationService.GoToH8();
}
