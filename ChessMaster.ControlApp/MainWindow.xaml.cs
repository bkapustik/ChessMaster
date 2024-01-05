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
using ChessMaster.ChessDriver.ChessStrategy;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ChessMaster.RobotDriver.State;
using Microsoft.UI.Xaml.Input;

namespace ChessMaster.ControlApp;


public sealed partial class MainWindow : Window
{
    private bool isA1Locked;
    private bool isH8Locked;
    private bool IsSpedUp;
    private bool IsConfigured;
    private int windowWidth = 500;
    private int windowHeight = 400;
    private string SelectedPort;
    private Vector3 desiredPosition;
    private bool initialPositionHasBeenSet = false;
    private Vector3 a1Position;
    private Vector3 h8Position;
    private ChessStrategyFacade selectedStrategy;
    private ChessRunner chessRunner;

    private bool isInitialized = false;
    private bool homingRequired = false;
    private bool alreadyExecuting = false;

    private DispatcherTimer timer;
    private int timerCounter = 0;

    public MainWindow()
    {
        this.InitializeComponent();
        Resize(windowWidth, windowHeight);

        timer = new DispatcherTimer();
    }

    private List<string> Ports = new()
    {
        "DUMMY"
    };

    private List<ChessStrategyFacade> Strategies = new();
        
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

    private void PortSelectedButton(object sender, RoutedEventArgs e)
    {
        try
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += MovementControl;
            timer.Tick += OnMovementButtonPressed;
            timer.Tick += UpdatePosition;
            timer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine(  ex.Message
                );
        }
        SelectedPort = (string)PortComboBox.SelectedValue;

        chessRunner = new ChessRunner(SelectedPort);

        Task.Run(chessRunner.Initialize);

        chessRunner.robot.SubscribeToCommandsCompletion(new CommandsCompletedEvent((object? o, RobotEventArgs e) =>
        {
            if (e.RobotState.RobotResponse == RobotResponse.NotInitialized)
            {
                throw new Exception("Replug");
            }
            if (e.RobotState.RobotResponse == RobotResponse.Initialized)
            {
                isInitialized = true;
            }
            if (isInitialized && !initialPositionHasBeenSet)
            {
                var position = e.RobotState.Position;
                desiredPosition = new Vector3(position.X, position.Y, position.Z);
                initialPositionHasBeenSet = true;
                return;
            }
            if (e.RobotState.RobotResponse == RobotResponse.HomingRequired)
            {
                homingRequired = true;
            }
            if (e.RobotState.RobotResponse == RobotResponse.Ok)
            {
                homingRequired = false;
                alreadyExecuting = false;
            }
            if (e.RobotState.RobotResponse == RobotResponse.AlreadyExecuting)
            {
                alreadyExecuting = true;
            }
        }));

        PortGrid.Visibility = Visibility.Collapsed;

        var strategies = chessRunner.GetStrategies();
        Strategies.AddRange(strategies);

        ConfigurationGrid.Visibility = Visibility.Visible;
        

        Resize(825, 350);
    }

    private void SpeedUpButton(object sender, RoutedEventArgs e)
    {
        IsSpedUp = !IsSpedUp;
    }

    private void MoveButton(object sender, RoutedEventArgs e)
    {
        if (!initialPositionHasBeenSet || homingRequired || !isInitialized || alreadyExecuting)
        {
            return;
        }
        var button = (Button)sender;

        var direction = GetDirectionVector(button.Name);
        if (IsSpedUp)
        {
            direction = direction * 10;
        }

        desiredPosition.X += direction.X;
        desiredPosition.Y += direction.Y;
    }

    private long counter = 0;

    private bool IsPressed = false;

    private void OnMovementButtonPressed(object sender, object args)
    {
        counter++;
    }
    private void UpdatePosition(object sender, object args)
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
    private void PressedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        IsPressed = true;
    }
    private void ReleasedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        IsPressed = false;
    }

    private void MovementControl(object sender, object args)
    {
        if (!initialPositionHasBeenSet || homingRequired || !isInitialized)
        {
            return;
        }

        var state = chessRunner.robot.GetState();

        if (state.MovementState == MovementState.Unknown)
        {
            return;
        }

        bool atDesired = chessRunner.robot.IsAtDesired(desiredPosition, state);

        if (!atDesired)
        {
            Task.Run(() => chessRunner.robot.Move(desiredPosition));
        }
    }

    private void LockCorner(object sender, RoutedEventArgs e)
    {
        if (homingRequired)
        {
            return;
        }

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
                    var state = chessRunner.robot.GetState();
                    if (chessRunner.robot.IsAtDesired(desiredPosition, state))
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
                    var state = chessRunner.robot.GetState();
                    if (chessRunner.robot.IsAtDesired(desiredPosition, state))
                    {
                        isH8Locked = true;
                        h8Position = desiredPosition;
                        DispatcherQueue.TryEnqueue(() => { button.Content = "Unlock H8"; });
                    }
                });
            }
        }
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

        Task.Run(async () => {
            var strategy = selectedStrategy.CreateStrategy();
            await chessRunner.PickStrategy(strategy);
            await chessRunner.Run();
        });
    }

    private void Home(object sender, RoutedEventArgs e)
    {
        Task.Run(chessRunner.robot.Home);
    }
}