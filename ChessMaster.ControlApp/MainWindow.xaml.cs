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

namespace ChessMaster.ControlApp;

public sealed partial class MainWindow : Window
{
    private int windowWidth = 1000;
    private int windowHeight = 700;
    private int timerCounter = 0;

    private ChessStrategyFacade selectedStrategy;

    public readonly DispatcherTimer Timer;

    public PositionSetupState SetupState = new PositionSetupState();

    public ChessRunner ChessRunner { get; private set; }

    public MainWindow()
    {
        this.InitializeComponent();
      
        Resize(windowWidth, windowHeight);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        Timer = new DispatcherTimer();
    }

    public void NavigateTo(Type page)
    {
        ContentFrame.Navigate(page);
    }

    public void RegisterKeyboardControl(VirtualKey key)
    {
        var holdableKey = new HoldableMoveKey(MainPage, key, SetupState);
    }

    public void ConfirmConfiguration()
    {
        if (!SetupState.IsA1Locked || !SetupState.IsH8Locked)
        {
            return;
        }

        ChessRunner.robot.InitializeChessBoard(SetupState.A1Position, SetupState.H8Position);

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

        SetupState = selectedRobot.GetSetupState();
        if (SetupState.RobotState == RobotResponse.Initialized)
        {
            ChessRunner.Initialize();
            ChessRunner.robot.InitializeChessBoard(SetupState.A1Position, SetupState.H8Position);
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
            SetupState.RobotState = RobotResponse.UnknownError;

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
            SetupState.RobotState = RobotResponse.Initialized;
            var position = e.RobotState.Position;
            SetupState.DesiredPosition = new Vector3(position.X, position.Y, position.Z);
        };
        ChessRunner.robot.Robot.CommandsSucceeded += (object o, RobotEventArgs e) =>
        {
            SetupState.RobotState = RobotResponse.Ok;
        };
        ChessRunner.robot.Robot.NotInitialized += (object o, RobotEventArgs e) => RequireRestart();
    }

    private void UpdateDisplayedPosition(object sender, object args)
    {
        if (timerCounter == 9)
        {
            Task.Run(() =>
            {
                var currentState = ChessRunner.robot.GetState();
                DispatcherQueue.TryEnqueue(() => { XValueLabel.Text = $"{currentState.x}"; });
                DispatcherQueue.TryEnqueue(() => { YValueLabel.Text = $"{currentState.y}"; });
                DispatcherQueue.TryEnqueue(() => { ZValueLabel.Text = $"{currentState.z}"; });
            });
        }

        timerCounter = (timerCounter + 1) % 10;
    }

    public void StartGame()
    {
        Task.Run(() =>
        {
            var strategy = selectedStrategy.CreateStrategy();
            ChessRunner.PickStrategy(strategy);
            ChessRunner.Run();
        });
    }
}
