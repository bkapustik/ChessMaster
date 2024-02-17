using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Robotic.Events;
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

            mainWindow.StartGame();
        });
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
