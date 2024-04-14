using ChessMaster.ChessDriver.Services;
using ChessMaster.ControlApp.Windows;
using ChessTracking.Core.ImageProcessing.PipelineParts.Events;
using ChessTracking.Core.Services;
using ChessTracking.Core.Tracking.State;
using Microsoft.Extensions.DependencyInjection;
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
    private IKinectService KinectService { get; set; }
    private GameController GameController { get; set; }
    private List<Tuple<string, Bitmap>> Data { get; set; }
    private int CurrentPosition { get; set; } = 0;
    public CalibrationSnapshotPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService = App.Services.GetRequiredService<IKinectService>();
        GameController = KinectService.GameController;

        var kinectWindow = (KinectWindow)e.Parameter;

        GameController.TrackingProcessor.OnSceneCalibrationSnapshotChanged += Update;

        kinectWindow.Closed += (object o, WindowEventArgs eventArgs) => { GameController.TrackingProcessor.OnSceneCalibrationSnapshotChanged -= Update; };
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
            LeftButton.Visibility = Visibility.Collapsed;
            RightButton.Visibility = Visibility.Collapsed;
            return;
        }

        var snapshot = e.Snapshot;

        DispatcherQueue.TryEnqueue(() =>
        {
            LeftButton.Visibility = Visibility.Visible;
            RightButton.Visibility = Visibility.Visible;
        });
        
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
        if (Data == null)
        {
            return;
        }
        if (CurrentPosition >= Data.Count)
            CurrentPosition = 0;
        if (CurrentPosition < 0)
            CurrentPosition = Data.Count - 1;

        if (Data[CurrentPosition].Item2 != null)
        {
            Task.Run(() =>
            {
                DisplayBitmapInWinUI(Data[CurrentPosition].Item2, VizualizationImage);
            });

            DispatcherQueue.TryEnqueue(() =>
            {
                NameLabel.Text = Data[CurrentPosition].Item1;
            });
        }
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
}
