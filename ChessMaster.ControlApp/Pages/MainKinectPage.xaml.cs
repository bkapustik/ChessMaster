using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Windows;
using ChessTracking.Core.Services.Events;
using ChessTracking.Core.Tracking.State;
using ChessTracking.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ChessMaster.ChessDriver.Events;
using System.Runtime.InteropServices.ObjectiveC;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class MainKinectPage : Page
{
    private IKinectService KinectService { get; set; }
    private KinectWindow KinectWindow { get; set; }

    public MainKinectPage()
    {
        this.InitializeComponent();
    }

    private void UpdateImmediateBoardBitmap(object o, BoardUpdatedEventArgs e)
    {
        Task.Run(() =>
        {
            DisplayBitmapInWinUI(e.Bitmap, ImmediateBoardStatePictureBox);
        });
    }

    private void UpdateAberagedBoardBitmap(object o, BoardUpdatedEventArgs e)
    {
        Task.Run(() =>
        {
            DisplayBitmapInWinUI(e.Bitmap, TrackedBoardStatePictureBox);
        });
    }

    private void UpdateBoardStateBitmap(object o, BoardUpdatedEventArgs e)
    {
        Task.Run(() =>
        {
            DisplayBitmapInWinUI(e.Bitmap, GameStatePictureBox);
        });
    }

    private void UpdateHistoryListBox(object o, KinectMoveDetectedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            GameHistoryListBox.Items.Add(e.UciMoveString);
        });
    }

    private void UpdateWhosPlaying(object o, WhosPlayingUpdatedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            WhosPlayingLabel.Text = e.ToString();
        });
    }

    private void UpdateProgramState(object o, ProgramStateEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (e.GameState.HasValue)
            {
                if (e.GameState != ChessTracking.Core.Game.GameState.StillPlaying)
                {
                    TrackingLogsListBox.Items.Add(e.GameState.Value.GetString());
                }
            }

            TrackingLogsListBox.Items.Add(e.ProgramState.GetString());
        });
    }

    private void ChangeGameValidationState(object o, GameValidationStateChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
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
        });
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService = App.Services.GetRequiredService<IKinectService>();

        KinectWindow = e.Parameter as KinectWindow;

        KinectService.GameController.TrackingProcessor.OnImmediateBoardUpdated += UpdateImmediateBoardBitmap;
        KinectService.GameController.TrackingProcessor.OnAveragedBoardUpdated += UpdateAberagedBoardBitmap;
        KinectService.GameController.TrackingProcessor.OnBoardStateUpdated += UpdateBoardStateBitmap;
        KinectService.OnKinectMoveDetected += UpdateHistoryListBox;
        KinectService.GameController.TrackingProcessor.OnWhosPlayingUpdated += UpdateWhosPlaying;
        KinectService.GameController.OnProgramStateChanged += UpdateProgramState;
        KinectService.GameController.TrackingProcessor.OnGameValidationStateChanged += ChangeGameValidationState;

        KinectWindow.Closed += (object o, WindowEventArgs e) =>
        {
            KinectService.GameController.TrackingProcessor.OnImmediateBoardUpdated -= UpdateImmediateBoardBitmap;
            KinectService.GameController.TrackingProcessor.OnAveragedBoardUpdated -= UpdateAberagedBoardBitmap;
            KinectService.GameController.TrackingProcessor.OnBoardStateUpdated -= UpdateBoardStateBitmap;
            KinectService.OnKinectMoveDetected -= UpdateHistoryListBox;
            KinectService.GameController.TrackingProcessor.OnWhosPlayingUpdated -= UpdateWhosPlaying;
            KinectService.GameController.OnProgramStateChanged -= UpdateProgramState;
            KinectService.GameController.TrackingProcessor.OnGameValidationStateChanged -= ChangeGameValidationState;
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

        KinectWindow.Closed += (object o, WindowEventArgs e) => kinectWindow.Close();

        AdvancedSettingsBtn.IsEnabled = false;

        kinectWindow.Closed += (object o, WindowEventArgs e) => { AdvancedSettingsBtn.IsEnabled = true; };
    }

    private void CalibrationSnapshotsButtonClick(object sender, RoutedEventArgs e)
    {
        KinectWindow kinectWindow = new(typeof(CalibrationSnapshotPage));
        kinectWindow.Activate();
        KinectWindow.Closed += (object o, WindowEventArgs e) => kinectWindow.Close();

        CalibrationSnapshotsButton.IsEnabled = false;

        kinectWindow.Closed += (object o, WindowEventArgs e) => { CalibrationSnapshotsButton.IsEnabled = true; };
    }

    private void DisplayBitmapInWinUI(Bitmap bitmap, Microsoft.UI.Xaml.Controls.Image imageControl)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
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
        KinectWindow.Closed += (object o, WindowEventArgs e) => kinectWindow.Close();

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
        Task.Run(() => KinectService.GameController.TrackingController.Start());
    }

    private void Recalibrate_Click(object sender, RoutedEventArgs e)
    {
        KinectService.GameController.TrackingController.Recalibrate();
    }

    private void StopTrackingBtn_Click(object sender, RoutedEventArgs e)
    {
        KinectService.GameController.TrackingController.Stop();
        StartTrackingBtn.IsEnabled = true;
        NewGameBtn.IsEnabled = true;
        LoadGameBtn.IsEnabled = true;
    }
}
