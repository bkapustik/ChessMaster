using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using System.Numerics;
using ChessMaster.ChessDriver.ChessStrategy;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ChessMaster.RobotDriver.State;
using Windows.System;

namespace ChessMaster.ControlApp;

public class PositionSetupState
{
    public RobotResponse RobotState { get; set; } = RobotResponse.NotInitialized;
    public bool IsSpedUp { get; set; }
    public bool IsA1Locked { get; set; }
    public bool IsH8Locked { get; set; }
    public Vector3 A1Position { get; set; }
    public Vector3 H8Position { get; set; }
    public Vector3 DesiredPosition { get; set; }
}

public class MoveHelper
{
    public static bool CanMove(RobotResponse robotState)
    {
        return robotState == RobotResponse.Ok || robotState == RobotResponse.Initialized;
    }

    public static Vector3 ChangeDesiredPosition(VirtualKey key, long ticksHeld, PositionSetupState state)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(key);
        if (state.IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(state.DesiredPosition.X + (direction.X * ticksHeld),
            state.DesiredPosition.Y + (direction.Y * ticksHeld),
            state.DesiredPosition.Z);
    }

    public static Vector3 ChangeDesiredPosition(string buttonName, long ticksHeld, PositionSetupState state)
    {
        ticksHeld /= 10;

        var direction = GetDirectionVector(buttonName);
        if (state.IsSpedUp)
        {
            direction = direction * 10 * ticksHeld;
        }

        return new Vector3(state.DesiredPosition.X + (direction.X * ticksHeld),
            state.DesiredPosition.Y + (direction.Y * ticksHeld),
            state.DesiredPosition.Z);
    }

    public static Vector2 GetDirectionVector(string buttonName)
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

    public static Vector2 GetDirectionVector(VirtualKey key)
    {
        switch (key)
        {
            case VirtualKey.Up:
                return new Vector2(0, 1);
            case VirtualKey.Down:
                return new Vector2(0, -1);
            case VirtualKey.Left:
                return new Vector2(-1, 0);
            case VirtualKey.Right:
                return new Vector2(1, 0);
            default:
                return new Vector2(0, 0);
        }
    }
}

public sealed partial class MainWindow : Window
{
    private int windowWidth = 500;
    private int windowHeight = 400;

    private int timerCounter = 0;

    private string SelectedPort;
    private PositionSetupState positionSetupState = new PositionSetupState();

    private ChessStrategyFacade selectedStrategy;
    private ChessRunner chessRunner;
    private DispatcherTimer timer;
    private List<string> Ports = new()
    {
        "DUMMY"
    };

    public MainWindow()
    {
        this.InitializeComponent();
        Resize(windowWidth, windowHeight);

        timer = new DispatcherTimer();
    }

    private List<ChessStrategyFacade> Strategies = new();

    private List<Button> allButtons;

    private void PortSelectionLoaded(object sender, RoutedEventArgs e)
    {
        PortComboBox.SelectedIndex = 0;
        var ports = SerialPort.GetPortNames();
        Ports.AddRange(ports);
    }

    private void StrategySelectionLoaded(object sender, RoutedEventArgs e)
    {
        StrategyComboBox.SelectedIndex = 0;
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

    private void Resize(int windowWidth, int windowHeight)
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));
        CenterToScreen(windowHandle);
    }

    private void RequireRestart()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            positionSetupState.RobotState = RobotResponse.UnknownError;

            ControlsWindow.Visibility = Visibility.Collapsed;
            RestartWindow.Visibility = Visibility.Visible;
        });
    }

    private void RequireHoming()
    {
        positionSetupState.RobotState = RobotResponse.HomingRequired;
        foreach (var button in allButtons)
        {
            if (button != HomeButton)
            {
                DispatcherQueue.TryEnqueue(() => button.IsEnabled = false);
            }
        }
    }

    private void PortSelectedButton(object sender, RoutedEventArgs e)
    {
        timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        timer.Tick += MovementControl;
        timer.Tick += UpdateDisplayedPosition;
        timer.Start();

        allButtons = new List<Button>()
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

        SelectedPort = (string)PortComboBox.SelectedValue;

        chessRunner = new ChessRunner(SelectedPort);

        Task.Run(chessRunner.Initialize);

        chessRunner.robot.Robot.RestartRequired += (object o, RobotEventArgs e) => RequireRestart();
        chessRunner.robot.Robot.Initialized += (object o, RobotEventArgs e) =>
        {
            positionSetupState.RobotState = RobotResponse.Initialized;
            var position = e.RobotState.Position;
            positionSetupState.DesiredPosition = new Vector3(position.X, position.Y, position.Z);
        };
        chessRunner.robot.Robot.HomingRequired += (object o, RobotEventArgs e) => RequireHoming();
        chessRunner.robot.Robot.CommandsSucceeded += (object o, RobotEventArgs e) =>
        {
            positionSetupState.RobotState = RobotResponse.Ok;
        };
        chessRunner.robot.Robot.NotInitialized += (object o, RobotEventArgs e) => RequireRestart();

        PortGrid.Visibility = Visibility.Collapsed;

        var strategies = chessRunner.GetStrategies();
        Strategies.AddRange(strategies);

        ConfigurationGrid.Visibility = Visibility.Visible;

        var holdableUpButton = new HoldableMoveButton(Up, positionSetupState);
        var holdableDownButton = new HoldableMoveButton(Down, positionSetupState);
        var holdableLeftButton = new HoldableMoveButton(Left, positionSetupState);
        var holdableRightButton = new HoldableMoveButton(Right, positionSetupState);

        var holdableUpKey = new HoldableMoveKey(MainPage, VirtualKey.Up, positionSetupState);
        var holdableDownKey = new HoldableMoveKey(MainPage, VirtualKey.Down, positionSetupState);
        var holdableLeftKey = new HoldableMoveKey(MainPage, VirtualKey.Left, positionSetupState);
        var holdableRightKey = new HoldableMoveKey(MainPage, VirtualKey.Right, positionSetupState);

        Resize(825, 350);
    }


    private void SpeedUpButton(object sender, RoutedEventArgs e)
    {
        positionSetupState.IsSpedUp = !positionSetupState.IsSpedUp;
    }

    private void UpdateDisplayedPosition(object sender, object args)
    {
        if (timerCounter == 9)
        {
            Task.Run(() =>
            {
                var currentState = chessRunner.robot.GetState();
                DispatcherQueue.TryEnqueue(() => { XValueLabel.Text = $"{currentState.x}"; });
                DispatcherQueue.TryEnqueue(() => { YValueLabel.Text = $"{currentState.y}"; });
                DispatcherQueue.TryEnqueue(() => { ZValueLabel.Text = $"{currentState.z}"; });
            });
        }

        timerCounter = (timerCounter + 1) % 10;
    }

    private void MovementControl(object sender, object args)
    {
        if (!CanMove())
        {
            return;
        }

        var state = chessRunner.robot.GetState();

        if (state.MovementState == MovementState.Unknown)
        {
            return;
        }

        bool atDesired = chessRunner.robot.IsAtDesired(positionSetupState.DesiredPosition, state);

        if (!atDesired)
        {
            Task.Run(() => chessRunner.robot.Move(positionSetupState.DesiredPosition));
        }
    }

    private void LockCorner(object sender, RoutedEventArgs e)
    {
        if (positionSetupState.RobotState == RobotResponse.HomingRequired)
        {
            return;
        }

        var button = sender as Button;
        if (button.Name == "LockA1")
        {
            if (positionSetupState.IsA1Locked)
            {
                positionSetupState.IsA1Locked = false;
                DispatcherQueue.TryEnqueue(() => { button.Content = "Lock A1"; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = chessRunner.robot.GetState();
                    if (chessRunner.robot.IsAtDesired(positionSetupState.DesiredPosition, state))
                    {
                        positionSetupState.IsA1Locked = true;
                        positionSetupState.A1Position = positionSetupState.DesiredPosition;
                        DispatcherQueue.TryEnqueue(() => { button.Content = "Unlock A1"; });
                    }
                });
            }
        }
        else if (button.Name == "LockH8")
        {
            if (positionSetupState.IsH8Locked)
            {
                positionSetupState.IsH8Locked = false;
                DispatcherQueue.TryEnqueue(() => { button.Content = "Lock H8"; });
            }
            else
            {
                Task.Run(() =>
                {
                    var state = chessRunner.robot.GetState();
                    if (chessRunner.robot.IsAtDesired(positionSetupState.DesiredPosition, state))
                    {
                        positionSetupState.IsH8Locked = true;
                        positionSetupState.H8Position = positionSetupState.DesiredPosition;
                        DispatcherQueue.TryEnqueue(() => { button.Content = "Unlock H8"; });
                    }
                });
            }
        }
    }

    private void ConfirmConfiguration(object sender, RoutedEventArgs e)
    {
        if (!positionSetupState.IsA1Locked || !positionSetupState.IsH8Locked)
        {
            return;
        }

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

    private void StrategySelectedButton(object sender, RoutedEventArgs e)
    {
        StrategyPickGrid.Visibility = Visibility.Collapsed;

        selectedStrategy = (ChessStrategyFacade)StrategyComboBox.SelectedValue;

        if (selectedStrategy.NeedsConfiguration)
        {
            PgnStrategyFilePick.Visibility = Visibility.Visible;
        }
        else
        {
            StartGame();
        }
    }

    private void SelectPgnFileButtonClicked(object sender, RoutedEventArgs e)
    {
        FileOpenPicker filePicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { ".pgn" },
        };

        var windowHandle = WindowNative.GetWindowHandle(this);

        InitializeWithWindow.Initialize(filePicker, windowHandle);

        Task.Run(async () =>
        {
            var selectedFile = await filePicker.PickSingleFileAsync();

            if (selectedFile is not null && !string.IsNullOrEmpty(selectedFile.Path))
            {
                DispatcherQueue.TryEnqueue(() => PgnFilePicker.Text = selectedFile.Path);
            }
        });
    }

    private void ConfirmPgnFile(object sender, RoutedEventArgs e)
    {
        selectedStrategy.Configure(PgnFilePicker.Text);

        StartGame();

        PgnStrategyFilePick.Visibility = Visibility.Collapsed;
    }

    private void StartGame()
    {
        AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 750, Height = 800 });

        Task.Run(async () =>
        {
            var strategy = selectedStrategy.CreateStrategy();
            await chessRunner.PickStrategy(strategy);
            await chessRunner.Run();
        });
    }

    private void Home(object sender, RoutedEventArgs e)
    {
        Task.Run(chessRunner.robot.Home);

        foreach (var button in allButtons)
        {
            if (button != HomeButton)
            {
                DispatcherQueue.TryEnqueue(() => button.IsEnabled = true);
            }
        }
    }

    private bool CanMove()
    {
        return MoveHelper.CanMove(positionSetupState.RobotState);
    }
}