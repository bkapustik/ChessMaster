using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using System.Numerics;
using ChessMaster.ChessDriver.ChessStrategy;
using WinRT.Interop;
using Windows.System;
using ChessMaster.ControlApp.Pages;
using ChessMaster.ChessDriver.Models;
using Microsoft.UI.Xaml.Controls;
using ChessMaster.RobotDriver.Events;
using ChessMaster.ChessDriver.Events;
using System.Collections.ObjectModel;

namespace ChessMaster.ControlApp;

public sealed partial class MainWindow : Window
{
    private int windowWidth = 1000;
    private int windowHeight = 700;
    private int timerCounter = 0;

    private bool continueInOldGame = false;
    public bool ChessBoardHasBeenInitialized { get; private set; } = false;

    private ChessStrategyFacade selectedStrategy;
    private Style TextBlockInGridStyle;

    public readonly DispatcherTimer Timer;

    public UIGameState UIGameState = new UIGameState();

    public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
    public bool MessagesInitialized { get; set; } = false;

    public ChessRunner ChessRunner { get; private set; }

    public MainWindow()
    {
        this.InitializeComponent();
      
        Resize(windowWidth, windowHeight);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        Timer = new DispatcherTimer();

        if (Application.Current.Resources.TryGetValue("TextBlockInGridStyle", out var style))
        {
            TextBlockInGridStyle = style as Style;
        }
    }

    public void ContinueGame()
    {
        continueInOldGame = true;
        NavigateTo(typeof(GamePage));
    }

    public void Home()
    {
        Task.Run(ChessRunner.robot.Home);
        UIGameState.DesiredPosition = ChessRunner.robot.Robot.Origin;
    }

    public void Play()
    {
        var strategy = selectedStrategy.CreateStrategy();

        if (UIGameState.GameState == GameState.InProgress && continueInOldGame)
        {
            continueInOldGame = false;
            ChessRunner.Resume();
        }
        else
        {
            Task.Run(() =>
            {
                if (ChessRunner.HadBeenStarted)
                {
                    if (strategy.CanAcceptOldContext)
                    {
                        throw new NotImplementedException();
                        ChessRunner.TryChangeStrategyWithContext(strategy, true);
                    }
                    else
                    {
                        if (ChessRunner.TryChangeStrategyWithContext(strategy, false))
                        {
                            ChessRunner.Start();
                        }
                    }
                }
                else
                {
                    ChessRunner.TryPickStrategy(strategy);
                    ChessRunner.Start();
                }
            });
        }
    }

    public void BackToConfiguration()
    {
        NavigateTo(typeof(ConfigurationPage));
    }

    public void AddMenuButton(Button button)
    {
        DynamicButtonPanel.Children.Add(button);
    }

    public void TryRemoveMenuButton(Button button) 
    {
        if (DynamicButtonPanel.Children.Contains(button))
        {
            DynamicButtonPanel.Children.Remove(button);
        }
    }

    public void AddLeftTabInfo(TextBlock label, TextBlock value)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        label.Style = TextBlockInGridStyle;
        value.Style = TextBlockInGridStyle;

        panel.Children.Add(label);
        panel.Children.Add(value);

        InformationPanel.Children.Add(panel);
    }

    public void NavigateTo(Type page)
    {
        if (ChessRunner != null)
        {
            if (ChessRunner.IsInitialized)
            {
                var currentState = ChessRunner.robot.GetState();
                UIGameState.DesiredPosition = currentState.Position;
            }
        }
        DynamicButtonPanel.Children.Clear();
        InformationPanel.Children.Clear();
        ContentFrame.Navigate(page);
    }

    public void RegisterKeyboardControl(VirtualKey key)
    {
        var holdableKey = new HoldableMoveKey(MainPage, key, UIGameState);
    }

    public void ConfirmConfiguration()
    {
        if (!UIGameState.IsA1Locked || !UIGameState.IsH8Locked)
        {
            return;
        }

        if (ChessBoardHasBeenInitialized)
        {
            ChessRunner.robot.ReconfigureChessBoard(UIGameState.A1Position, UIGameState.H8Position);
        }
        else
        {
            ChessBoardHasBeenInitialized = true;
            ChessRunner.robot.TryInitializeChessBoard(UIGameState.A1Position, UIGameState.H8Position);
        }

        NavigateTo(typeof(SelectStrategyPage));
    }

    public void PickPgnFile(string file)
    {
        selectedStrategy.Configure(file);

        NavigateTo(typeof(GamePage));
    }

    public void PickStrategy(ChessStrategyFacade chessStrategyFacade)
    {
        selectedStrategy = chessStrategyFacade;

        if (!selectedStrategy.NeedsConfiguration)
        {
            NavigateTo(typeof(GamePage));
        }
        else
        {
            NavigateTo(typeof(PgnFilePickerPage));
        }
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

    public void Resize(int windowWidth, int windowHeight)
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));
        CenterToScreen(windowHandle);
    }

    public void SelectPort(RobotDTO selectedRobot, string portName)
    {
        InitializeTimer();
        ChessRunner = new ChessRunner(selectedRobot.GetRobot(portName));
        InitializeRobot();

        UIGameState = selectedRobot.GetSetupState();
        if (UIGameState.RobotState == RobotResponse.Initialized)
        {
            ChessRunner.Initialize();
            ChessRunner.robot.TryInitializeChessBoard(UIGameState.A1Position, UIGameState.H8Position);
            var selectedStrategy = new MockPgnStrategyFacade();
            PickStrategy(selectedStrategy);
        }
        else
        {
            NavigateTo(typeof(ConfigurationPage));
        }
    }

    private void RequireRestart()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            UIGameState.RobotState = RobotResponse.UnknownError;

            ControlsWindow.Visibility = Visibility.Collapsed;
            RestartWindow.Visibility = Visibility.Visible;
        });
    }

    private void InitializeTimer()
    {
        Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        Timer.Tick += UpdateDisplayedPosition;
        Timer.Start();
    }

    private void InitializeRobot()
    {
        ChessRunner.robot.Robot.RestartRequired += (object o, RobotEventArgs e) => RequireRestart();
        ChessRunner.robot.Robot.Initialized += (object o, RobotEventArgs e) =>
        {
            UIGameState.RobotState = RobotResponse.Initialized;
            var position = e.RobotState.Position;
            UIGameState.DesiredPosition = new Vector3(position.X, position.Y, position.Z);
        };
        ChessRunner.robot.Robot.CommandsSucceeded += (object o, RobotEventArgs e) =>
        {
            UIGameState.RobotState = RobotResponse.Ok;
        };
        ChessRunner.robot.Robot.NotInitialized += (object o, RobotEventArgs e) => RequireRestart();
        ChessRunner.OnGameStateChanged += (object o, GameStateEventArgs e) =>
        {
            UIGameState.GameState = e.GameState;
        };
    }

    private void UpdateDisplayedPosition(object sender, object args)
    {
        if (timerCounter == 1)
        {
            Task.Run(() =>
            {
                var currentState = ChessRunner.robot.GetState();

                UIGameState.RobotState = currentState.RobotResponse;
                UIGameState.MovementState = currentState.MovementState;

                DispatcherQueue.TryEnqueue(() => { XValueLabel.Text = $"{currentState.x}"; });
                DispatcherQueue.TryEnqueue(() => { YValueLabel.Text = $"{currentState.y}"; });
                DispatcherQueue.TryEnqueue(() => { ZValueLabel.Text = $"{currentState.z}"; });
            });
        }

        timerCounter = (timerCounter + 1) % 2;
    }
}
