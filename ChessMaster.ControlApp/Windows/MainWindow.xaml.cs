using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using ChessMaster.RobotDriver.Robotic;
using System.Threading.Tasks;
using System.Numerics;
using ChessMaster.ChessDriver.ChessStrategy;
using WinRT.Interop;
using Windows.System;
using ChessMaster.ControlApp.Pages;
using Microsoft.UI.Xaml.Controls;
using ChessMaster.RobotDriver.Events;
using ChessMaster.ChessDriver.Events;
using ChessMaster.ControlApp.Windows;
using Windows.Graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Services;
using System.Reflection;

namespace ChessMaster.ControlApp;

public sealed partial class MainWindow : Window
{
    private int windowWidth = 1000;
    private int windowHeight = 700;
    private int timerCounter = 0;

    private bool continueInOldGame = false;
    private ChessStrategyFacade selectedStrategy;
    private Style TextBlockInGridStyle;

    private readonly IChessRunner chessRunner;
    private readonly IUIRobotService robotService;
    private readonly IConfigurationService configurationService;

    public MainWindow()
    {
        InitializeComponent();

        chessRunner = App.Services.GetRequiredService<IChessRunner>();
        robotService = App.Services.GetRequiredService<IUIRobotService>();
        configurationService = App.Services.GetRequiredService<IConfigurationService>();
        ExtendsContentIntoTitleBar = true;
        
        Resize(windowWidth, windowHeight);
        SetTitleBar(AppTitleBar);
        
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

    public void Play()
    {
        var strategy = selectedStrategy.CreateStrategy();

        if (robotService.UIGameState.GameState == GameState.InProgress && continueInOldGame)
        {
            continueInOldGame = false;
            chessRunner.Resume();
        }
        else
        {
            Task.Run(() =>
            {
                if (this.chessRunner.GameHadBeenStarted)
                {
                    if (strategy.CanAcceptOldContext)
                    {
                        throw new NotImplementedException();
                        chessRunner.TryChangeStrategyWithContext(strategy, true);
                    }
                    else
                    {
                        if (chessRunner.TryChangeStrategyWithContext(strategy, false))
                        {
                            chessRunner.Start();
                        }
                    }
                }
                else
                {
                    chessRunner.TryPickStrategy(strategy);
                    chessRunner.Start();
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
        if (chessRunner != null)
        {
            if (chessRunner.RobotIsInitialized)
            {
                var currentState = chessRunner.GetRobotState();
                configurationService.RobotDesiredPosition = currentState.Position;
            }
        }
        DynamicButtonPanel.Children.Clear();
        InformationPanel.Children.Clear();
        ContentFrame.Navigate(page);
    }

    public void RegisterKeyboardControl(KeyEventHandler handler)
    {
        MainPage.KeyDown += handler;
    }
    public void RegisterHoldableKeyboardControl(VirtualKey key)
    {
        var holdableKey = new HoldableMoveKey(MainPage, key);
    }

    public void ConfirmConfiguration()
    {
        if (!configurationService.A1Corner.Locked || !configurationService.H8Corner.Locked)
        {
            return;
        }

        if (chessRunner.ChessBoardInitialized)
        {
            chessRunner.ReconfigureChessBoard(configurationService.A1Corner.Position, configurationService.H8Corner.Position);
        }
        else
        {
            robotService.UIGameState.ChessBoardInitialized = true;
            chessRunner.InitializeChessBoard(configurationService.A1Corner.Position, configurationService.H8Corner.Position);
        }

        NavigateTo(typeof(SelectStrategyPage));
    }

    public void PickFile(string file)
    {
        selectedStrategy.Configure(file);

        NavigateTo(typeof(GamePage));
    }

    public void PickStrategy(ChessStrategyFacade chessStrategyFacade)
    {
        selectedStrategy = chessStrategyFacade;

        if (selectedStrategy.NeedsKinectConfiguration)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                KinectWindow kinectWindow = new(typeof(MainKinectPage));
                kinectWindow.Activate();

                selectedStrategy.Configure(App.Services.GetRequiredService<IKinectService>());
            });
        }

        if (!selectedStrategy.NeedsFileConfiguration)
        {
            NavigateTo(typeof(GamePage));
        }
        else
        {
            configurationService.AcceptedFileTypes = selectedStrategy.AcceptedFileTypes;

            NavigateTo(typeof(FilePickerPage));
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
        AppWindow.Resize(new SizeInt32(windowWidth, windowHeight));
        CenterToScreen(windowHandle);
    }

    public void PortSelected()
    {
        InitializeTimer();
        ConfigureRobotEventReactions();

        if (robotService.UIGameState.RobotState == RobotResponse.Initialized)
        {
            chessRunner.Initialize();
            chessRunner.InitializeChessBoard(configurationService.A1Corner.Position, configurationService.H8Corner.Position);
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
            robotService.UIGameState.RobotState = RobotResponse.UnknownError;

            ControlsWindow.Visibility = Visibility.Collapsed;
            RestartWindow.Visibility = Visibility.Visible;
        });
    }

    private void InitializeTimer()
    {
        robotService.Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        robotService.Timer.Tick += UpdateDisplayedPosition;
        robotService.Timer.Start();
    }

    private void ConfigureRobotEventReactions()
    {
        chessRunner.RobotStateEvents.RestartRequired += (object o, RobotEventArgs e) => RequireRestart();
        chessRunner.RobotStateEvents.Initialized += (object o, RobotEventArgs e) =>
        {
            robotService.UIGameState.RobotState = RobotResponse.Initialized;
            var position = e.RobotState.Position;
            configurationService.RobotDesiredPosition = new Vector3(position.X, position.Y, position.Z);
        };
        chessRunner.RobotStateEvents.CommandsSucceeded += (object o, RobotEventArgs e) =>
        {
            robotService.UIGameState.RobotState = RobotResponse.Ok;
        };
        chessRunner.RobotStateEvents.NotInitialized += (object o, RobotEventArgs e) => RequireRestart();
        chessRunner.OnGameStateChanged += (object o, GameStateEventArgs e) =>
        {
            robotService.UIGameState.GameState = e.GameState;
        };
    }

    private void UpdateDisplayedPosition(object sender, object args)
    {
        if (timerCounter == 1)
        {
            Task.Run(() =>
            {
                var currentState = chessRunner.GetRobotState();

                robotService.UIGameState.RobotState = currentState.RobotResponse;
                robotService.UIGameState.MovementState = currentState.MovementState;

                DispatcherQueue.TryEnqueue(() => { XValueLabel.Text = $"{currentState.x}"; });
                DispatcherQueue.TryEnqueue(() => { YValueLabel.Text = $"{currentState.y}"; });
                DispatcherQueue.TryEnqueue(() => { ZValueLabel.Text = $"{currentState.z}"; });
            });
        }

        timerCounter = (timerCounter + 1) % 2;
    }
}
