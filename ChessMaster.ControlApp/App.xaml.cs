using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Pages;
using ChessMaster.ControlApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace ChessMaster.ControlApp;

public partial class App : Application
{
    private IServiceProvider serviceProvider;

    public App()
    {
        this.InitializeComponent();
    }

    public static IServiceProvider Services
    {
        get
        {
            IServiceProvider serviceProvider = ((App)Current).serviceProvider
                ?? throw new InvalidOperationException("The service provider is not initialized");
            return serviceProvider;
        }
    }

    private static IServiceProvider ConfigureServices() =>
        new ServiceCollection()
            .AddSingleton<IKinectService, KinectService>()
            .AddSingleton<IUIRobotService, UIRobotService>()
            .AddSingleton<IChessRunner, ChessRunner>()
            .AddSingleton<IConfigurationService, ConfigurationService>()
        .BuildServiceProvider(true);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (serviceProvider == null)
        {
            serviceProvider = ConfigureServices();
        }

        MainWindow = new MainWindow();
        MainWindow.NavigateTo(typeof(PortSelectionPage));

        m_window = MainWindow;

        MainWindow.Closed += (object o, WindowEventArgs e) =>
        {
            var kinectService = serviceProvider.GetRequiredService<IKinectService>();
            kinectService.GameController.TrackingController.Dispose();
        };

        m_window.Activate();
    }

    private Window m_window;

    public static MainWindow MainWindow;
}
