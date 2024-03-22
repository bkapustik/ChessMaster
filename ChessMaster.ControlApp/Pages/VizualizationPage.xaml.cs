using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using ChessTracking.Core.Services.Events;
using ChessMaster.ControlApp.Windows;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ChessMaster.ChessDriver.Services;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class VizualizationPage : Page
{
    private IKinectService KinectService { get; set; }
    public VizualizationPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService = App.Services.GetRequiredService<IKinectService>();

        KinectService.GameController.TrackingProcessor.OnVizualizationUpdated += DisplayVizulization;

        var kinectWindow = (KinectWindow)e.Parameter;
        kinectWindow.Closed += (object o, WindowEventArgs eventArgs) => { KinectService.GameController.TrackingProcessor.OnVizualizationUpdated -= DisplayVizulization; };
    }

    private void DisplayVizulization(object o, VizualizationUpdateEventArgs e)
    {
        var bitmap = e.Bitmap;

        if (bitmap != null)
        {
            Task.Run(() =>
            {
                DisplayBitmapInWinUI(bitmap, VizualizationImage);
            });
        }
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
}
