using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Helpers;
using ChessMaster.ControlApp.Services;
using ChessMaster.RobotDriver.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class GamePage : Page, INotifyPropertyChanged
{
    private MainWindow mainWindow;
    private bool isPaused = false;
    private Button pauseButton;
    private Button finishMoveButton;
    private Button changeStrategyButton;
    private Button reconfigureButton;
    private IChessRunner chessRunner;
    private IUIRobotService robotService;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<string> Messages
    {
        get 
        {
            if (robotService == null)
            {
                robotService = App.Services.GetRequiredService<IUIRobotService>();
            }
            return robotService.GameMessages; 
        }
        set
        {
            robotService.GameMessages = value;
            OnPropertyChanged(nameof(Messages));
        }
    }

    public GamePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        mainWindow.SizeChanged -= MainWindow_SizeChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        MessagesList.ItemsSource = Messages;
        robotService = App.Services.GetRequiredService<IUIRobotService>();
        chessRunner = App.Services.GetRequiredService<IChessRunner>();


        if (!robotService.MessagesInitialized)
        {
            robotService.MessagesInitialized = true;

            Task.Run(() =>
            {
                chessRunner.OnMessageLogged += (object o, LogEventArgs e) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        Messages.Add(e.Message);
                    });
                };
            });
        }

        var controlFactory = new ControlFactory(mainWindow);

        changeStrategyButton = controlFactory.CreateChangeStrategyButton();
        reconfigureButton = controlFactory.CreateBackToConfigurationButton();

        mainWindow.Play();

        MessagesList.Width = mainWindow.windowWidth - 200;
        MessagesList.Height = mainWindow.windowHeight - 150;

        mainWindow.SizeChanged += MainWindow_SizeChanged;

        pauseButton = ControlFactory.CreateMenuButton("Pause");
        pauseButton.Click += PauseClick;

        finishMoveButton = ControlFactory.CreateMenuButton("Finish Move");
        finishMoveButton.Click += FinishMoveClick;

        mainWindow.AddMenuButton(pauseButton);
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        MessagesList.Width = args.Size.Width - 200;
        MessagesList.Height = args.Size.Height - 150;
    }

    private void FinishMoveClick(object o, RoutedEventArgs e)
    {
        chessRunner.FinishMove();
        
        mainWindow.TryRemoveMenuButton(finishMoveButton);
        mainWindow.AddMenuButton(reconfigureButton);
        mainWindow.AddMenuButton(changeStrategyButton);
    }

    private void PauseClick(object o, RoutedEventArgs e)
    {
        if (isPaused)
        {
            chessRunner.Resume();
            mainWindow.TryRemoveMenuButton(finishMoveButton);
            mainWindow.TryRemoveMenuButton(changeStrategyButton);
            mainWindow.TryRemoveMenuButton(reconfigureButton);
            pauseButton.Content = "Pause";
        }
        else
        {
            chessRunner.Pause();
            mainWindow.AddMenuButton(finishMoveButton);
            pauseButton.Content = "Resume";
        }
        isPaused = !isPaused;
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
