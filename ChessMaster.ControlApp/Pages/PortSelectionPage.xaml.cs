using ChessMaster.ChessDriver;
using ChessMaster.ControlApp.Helpers;
using ChessMaster.ControlApp.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.IO.Ports;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class PortSelectionPage : Page
{
    private MainWindow mainWindow;

    private RobotPicker robotPicker = new();
    private ChessRunner chessRunner;
    private UIRobotService robotService;
    private ConfigurationService configurationService;

    public PortSelectionPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        chessRunner = ChessRunner.Instance;
        robotService = UIRobotService.Instance;
        configurationService = ConfigurationService.Instance;

        PortComboBox.SelectedIndex = 0;
        var ports = SerialPort.GetPortNames();

        foreach (var port in ports)
        {
            robotPicker.AddPort(port);
        }
    }

    private void PortSelectedButton(object sender, RoutedEventArgs e)
    {
        var selectedPort = PortComboBox.SelectedItem as string;
        
        var robot = robotPicker.GetRobot(selectedPort);

        chessRunner.SelectPort(robot.GetRobot(selectedPort));

        robotService.UIGameState = robot.GetSetupState();

        var configurationState = robot.GetConfiguration();

        configurationService.RobotDesiredPosition = configurationState.RobotDesiredPosition;
        configurationService.A1Corner.Locked = configurationState.A1Locked;
        configurationService.H8Corner.Locked = configurationState.H8Locked;
        configurationService.H8Corner.Position = configurationState.H8Position;
        configurationService.A1Corner.Position = configurationState.A1Position;

        mainWindow.PortSelected();
    }
}
