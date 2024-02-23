using ChessMaster.ControlApp.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.IO.Ports;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class PortSelectionPage : Page
{
    private MainWindow mainWindow;

    private readonly RobotPicker robotPicker = new();

    public PortSelectionPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;

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

        mainWindow.SelectPort(robot, selectedPort);
    }
}
