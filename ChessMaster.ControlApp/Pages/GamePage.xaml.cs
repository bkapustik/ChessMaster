using ChessMaster.ChessDriver;
using ChessMaster.ControlApp.Helpers;
using ChessMaster.RobotDriver.Events;
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
    private ChessRunner chessRunner;
    private UIRobotService robotService;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<string> Messages
    {
        get 
        {
            if (robotService == null)
            { 
                robotService = UIRobotService.Instance;
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        MessagesList.ItemsSource = Messages;
        robotService = UIRobotService.Instance;
        chessRunner = ChessRunner.Instance;


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

        pauseButton = ControlFactory.CreateMenuButton("Pause");
        pauseButton.Click += PauseClick;

        finishMoveButton = ControlFactory.CreateMenuButton("Finish Move");
        finishMoveButton.Click += FinishMoveClick;

        mainWindow.AddMenuButton(pauseButton);
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
