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

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<string> Messages
    {
        get { return mainWindow.Messages; }
        set
        {
            mainWindow.Messages = value;
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

        if (!mainWindow.MessagesInitialized)
        {
            mainWindow.MessagesInitialized = true;

            Task.Run(() =>
            {
                mainWindow.ChessRunner.OnMessageLogged += (object o, LogEventArgs e) =>
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
        mainWindow.ChessRunner.FinishMove();
        
        mainWindow.TryRemoveMenuButton(finishMoveButton);
        mainWindow.AddMenuButton(reconfigureButton);
        mainWindow.AddMenuButton(changeStrategyButton);
    }

    private void PauseClick(object o, RoutedEventArgs e)
    {
        if (isPaused)
        {
            mainWindow.ChessRunner.Resume();
            mainWindow.TryRemoveMenuButton(finishMoveButton);
            mainWindow.TryRemoveMenuButton(changeStrategyButton);
            mainWindow.TryRemoveMenuButton(reconfigureButton);
            pauseButton.Content = "Pause";
        }
        else
        {
            mainWindow.ChessRunner.Pause();
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
