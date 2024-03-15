using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System;
using ChessMaster.ControlApp.Services;
using Microsoft.UI.Xaml.Navigation;
using ChessTracking.Core.Services.Events;
using ChessMaster.ControlApp.Windows;
using Microsoft.UI.Xaml;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class VizualizationPage : Page
{
    private UIKinectService KinectService { get; set; }
    public VizualizationPage()
    {
        this.InitializeComponent();

        KinectService = UIKinectService.Instance;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService.GameController.TrackingProcessor.OnVizualizationUpdated += DisplayVizulization;

        var kinectWindow = (KinectWindow)e.Parameter;

        kinectWindow.Closed += (object o, WindowEventArgs e) =>
        {
            KinectService.GameController.TrackingProcessor.OnVizualizationUpdated -= DisplayVizulization;
        };
    }

    private void DisplayVizulization(object o, VizualizationUpdateEventArgs e)
    {
        var bitmap = e.Bitmap;

        if (bitmap != null)
        {
            Task.Run(async () =>
            {
                await DisplayBitmapInWinUI(bitmap, VizualizationImage);
            });
        }
    }

    private async Task DisplayBitmapInWinUI(Bitmap bitmap, Microsoft.UI.Xaml.Controls.Image imageControl)
    {
        using (var memoryStream = new MemoryStream())
        {
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());

            imageControl.Source = bitmapImage;
        }
    }
}
