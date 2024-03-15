using ChessMaster.ControlApp.Services;
using ChessMaster.ControlApp.Windows;
using ChessTracking.Core.Tracking.State;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class MainKinectPage : Page
{
    private KinectWindow windowHandler;
    private SceneCalibrationSnapshot Snapshot { get; set; }
    private UIKinectService UIKinectService { get; set; }

    public MainKinectPage()
    {
        this.InitializeComponent();
        UIKinectService = UIKinectService.Instance;

        BitmapImage bitmapImage = new BitmapImage();
        Uri uri = new Uri("https://upload.wikimedia.org/wikipedia/commons/b/b6/Image_created_with_a_mobile_phone.png");
        bitmapImage.UriSource = uri;

        GameStatePictureBox.Source = bitmapImage;
        ImmediateBoardStatePictureBox.Source = bitmapImage;
        TrackedBoardStatePictureBox.Source = bitmapImage;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        windowHandler = (KinectWindow)e.Parameter;

        windowHandler.CreateNewWindow(typeof(VizualizationPage));
    }

    private void AdvancedSettingsButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        windowHandler.CreateNewWindow(typeof(AdvancedSettingsPage));
    }

    private void CalibrationSnapshotsButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        windowHandler.CreateNewWindow(typeof(CalibrationSnapshotPage));
    }
}
