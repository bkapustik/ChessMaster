using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Windows;
using ChessTracking.Core.Services.Events;
using ChessTracking.Core.Tracking.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class MainKinectPage : Page
{
    private SceneCalibrationSnapshot Snapshot { get; set; }
    private IKinectService KinectService { get; set; }

    public MainKinectPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService = App.Services.GetRequiredService<IKinectService>();

        KinectService.GameController.TrackingProcessor.OnImmediateBoardUpdated += (object o, BoardUpdatedEventArgs e) =>
        {
            Task.Run(() =>
            {
                DisplayBitmapInWinUI(e.Bitmap, ImmediateBoardStatePictureBox);
            });
        };

        KinectService.GameController.TrackingProcessor.OnAveragedBoardUpdated += (object o, BoardUpdatedEventArgs e) =>
        {
            Task.Run(() =>
            {
                DisplayBitmapInWinUI(e.Bitmap, TrackedBoardStatePictureBox);
            });
        };

        KinectService.GameController.TrackingProcessor.OnBoardStateUpdated += (object o, BoardUpdatedEventArgs e) =>
        {
            Task.Run(() =>
            {
                DisplayBitmapInWinUI(e.Bitmap, GameStatePictureBox);
            });
        };

        KinectService.GameController.TrackingProcessor.OnRecordStateUpdated += (object o, RecordStateUpdatedEventArgs e) =>
        {
            GameHistoryListBox.Items.Clear();
            e.RecordOfGame.ForEach(x => GameHistoryListBox.Items.Add(x));
        };

        KinectService.GameController.TrackingProcessor.OnWhosPlayingUpdated += (object o, WhosPlayingUpdatedEventArgs e) =>
        {
            WhosPlayingLabel.Text = e.ToString();
        };

        KinectService.GameController.OnProgramStateChanged += (object o, ProgramStateEventArgs e) =>
        {

        };

        KinectService.GameController.TrackingProcessor.OnGameValidationStateChanged += (object o, GameValidationStateChangedEventArgs e) =>
        {
            if (!e.IsValid.HasValue)
            {
                ValidationStateBtn.Text = "Validation State";
            }
            else if (e.IsValid.Value)
            {
                ValidationStateBtn.Text = "Valid State";
            }
            else
            {
                ValidationStateBtn.Text = "Invalid State";
            }
        };


        EndGameBtn.IsEnabled = false;
        SaveGameBtn.IsEnabled = false;
        StartTrackingBtn.IsEnabled = false;
        Recalibrate.IsEnabled = false;
        StopTrackingBtn.IsEnabled = false;
    }

    private void AdvancedSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        KinectWindow kinectWindow = new(typeof(AdvancedSettingsPage));
        kinectWindow.Activate();

        AdvancedSettingsBtn.IsEnabled = false;

        kinectWindow.Closed += (object o, WindowEventArgs e) => { AdvancedSettingsBtn.IsEnabled = true; };
    }

    private void CalibrationSnapshotsButtonClick(object sender, RoutedEventArgs e)
    {
        KinectWindow kinectWindow = new(typeof(CalibrationSnapshotPage));
        kinectWindow.Activate();

        CalibrationSnapshotsButton.IsEnabled = false;

        kinectWindow.Closed += (object o, WindowEventArgs e) => { CalibrationSnapshotsButton.IsEnabled = true; };
    }

    private void DisplayBitmapInWinUI(Bitmap bitmap, Microsoft.UI.Xaml.Controls.Image imageControl)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(memoryStream.AsRandomAccessStream());

                imageControl.Source = bitmapImage;
            }
        });
    }

    private void DisplayVizualization_Click(object sender, RoutedEventArgs e)
    {
        KinectWindow kinectWindow = new(typeof(VizualizationPage));
        kinectWindow.Activate();

        DisplayVizualization.IsEnabled = false;

        kinectWindow.Closed += (object o, WindowEventArgs e) => { DisplayVizualization.IsEnabled = true; };
    }

    private void NewGameBtn_Click(object sender, RoutedEventArgs e)
    {
        KinectService.GameController.NewGame();

        NewGameBtn.IsEnabled = false;
        LoadGameBtn.IsEnabled = false;

        SaveGameBtn.IsEnabled = true;
        EndGameBtn.IsEnabled = true;
        StartTrackingBtn.IsEnabled = true;
    }

    private void StartTrackingBtn_Click(object sender, RoutedEventArgs e)
    {
        SaveGameBtn.IsEnabled = false;
        EndGameBtn.IsEnabled = false;
        StartTrackingBtn.IsEnabled = false;

        Recalibrate.IsEnabled = true;
        StopTrackingBtn.IsEnabled = true;

        KinectService.GameController.TrackingController.Start();
    }

    private void Recalibrate_Click(object sender, RoutedEventArgs e)
    {
        KinectService.GameController.TrackingController.Recalibrate();
    }

    private void StopTrackingBtn_Click(object sender, RoutedEventArgs e)
    {
        KinectService.GameController.TrackingController.Stop();
        StartTrackingBtn.IsEnabled = true;
    }
}
