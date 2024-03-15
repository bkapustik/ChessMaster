using ChessMaster.ControlApp.Services;
using ChessMaster.ControlApp.Windows;
using ChessTracking.Core.ImageProcessing.PipelineParts.Events;
using ChessTracking.Core.Services;
using ChessTracking.Core.Tracking.State;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class CalibrationSnapshotPage : Page
{
    private KinectWindow KinectWindow { get; set; }
    private UIKinectService KinectService { get; set; }
    private GameController GameController { get; set; }
    private List<Tuple<string, Bitmap>> Data { get; set; }
    private int CurrentPosition { get; set; } = 0;
    public CalibrationSnapshotPage()
    {
        this.InitializeComponent();

        KinectService = UIKinectService.Instance;
        GameController = KinectService.GameController;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        GameController.TrackingProcessor.OnSceneCalibrationSnapshotChanged += Update;

        KinectWindow = (KinectWindow)e.Parameter;

        KinectWindow.Closed += (object o, WindowEventArgs e) =>
        {
            GameController.TrackingProcessor.OnSceneCalibrationSnapshotChanged -= Update;
        };
    }

    private void GenerateListFromSnapshot(SceneCalibrationSnapshot snapshot)
    {
        Data = new List<Tuple<string, Bitmap>>()
        {
            new Tuple<string, Bitmap>(nameof(snapshot.BinarizationImage), snapshot.BinarizationImage),
            new Tuple<string, Bitmap>(nameof(snapshot.CannyImage), snapshot.CannyImage),
            new Tuple<string, Bitmap>(nameof(snapshot.GrayImage), snapshot.GrayImage),
            new Tuple<string, Bitmap>(nameof(snapshot.MaskedColorImage), snapshot.MaskedColorImage)
        };
    }

    private void Update(object o, SceneCalibrationSnapshotEventArgs e)
    {
        if (e == null)
        {
            NameLabel.Text = "No data arrived";
            LeftButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            RightButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            return;
        }

        var snapshot = e.Snapshot;

        LeftButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        RightButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        GenerateListFromSnapshot(snapshot);
        CurrentPosition = 0;
        UpdatePicturebox();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CurrentPosition--;
        UpdatePicturebox();
    }

    private void Button_Click_1(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CurrentPosition++;
        UpdatePicturebox();
    }

    private void UpdatePicturebox()
    {
        if (CurrentPosition >= Data.Count)
            CurrentPosition = 0;
        if (CurrentPosition < 0)
            CurrentPosition = Data.Count - 1;

        if (Data[CurrentPosition].Item2 != null)
        {

            Task.Run(async () =>
            {
                await DisplayBitmapInWinUI(Data[CurrentPosition].Item2, VizualizationImage);
            });
            NameLabel.Text = Data[CurrentPosition].Item1;
        }
    }

    private async Task DisplayBitmapInWinUI(Bitmap bitmap, Microsoft.UI.Xaml.Controls.Image imageControl)
    {
        using (var memoryStream = new MemoryStream())
        {
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());

            imageControl.Source = bitmapImage;
        }
    }
}
