using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Robotic.Events;
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
    private ObservableCollection<string> _messages = new ObservableCollection<string>();
    private bool isPaused = false;
    private Button pauseButton;
    private Button finishMoveButton;
    private Button changeStrategyButton;
    private Button reconfigureButton;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<string> Messages
    {
        get { return _messages; }
        set
        {
            _messages = value;
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

        mainWindow.StartGame();

        pauseButton = CreateMenuButton("Pause");
        pauseButton.Click += PauseClick;

        finishMoveButton = CreateMenuButton("Finish Move");
        finishMoveButton.Click += FinishMoveClick;

        changeStrategyButton = CreateMenuButton("Change Strategy");
        changeStrategyButton.Click += ChangeStrategyClick;

        mainWindow.AddMenuButton(pauseButton);
    }

    private void ChangeStrategyClick(object o, RoutedEventArgs e)
    {
        mainWindow.NavigateTo(typeof(SelectStrategyPage));
    }

    private void FinishMoveClick(object o, RoutedEventArgs e)
    {
        mainWindow.ChessRunner.FinishMove();
        mainWindow.TryRemoveMenuButton(finishMoveButton);
        mainWindow.AddMenuButton(changeStrategyButton);
    }

    private void PauseClick(object o, RoutedEventArgs e)
    {
        if (isPaused)
        {
            mainWindow.ChessRunner.Resume();
            mainWindow.TryRemoveMenuButton(finishMoveButton);
            mainWindow.TryRemoveMenuButton(changeStrategyButton);
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

    private Button CreateMenuButton(string content) =>
        new Button
        {
            Margin = new Thickness(5),
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
            Height = 40
        };
}
