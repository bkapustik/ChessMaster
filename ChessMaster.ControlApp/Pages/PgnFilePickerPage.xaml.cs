using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class PgnFilePickerPage : Page
{
    private MainWindow mainWindow;

    public PgnFilePickerPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
    }

    private void SelectPgnFileButtonClicked(object sender, RoutedEventArgs e)
    {
        FileOpenPicker filePicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            FileTypeFilter = { ".pgn" },
        };

        var windowHandle = WindowNative.GetWindowHandle(mainWindow);

        InitializeWithWindow.Initialize(filePicker, windowHandle);

        Task.Run(async () =>
        {
            var selectedFile = await filePicker.PickSingleFileAsync();

            if (selectedFile is not null && !string.IsNullOrEmpty(selectedFile.Path))
            {
                mainWindow.DispatcherQueue.TryEnqueue(() => PgnFilePicker.Text = selectedFile.Path);
            }
        });
    }

    private void ConfirmPgnFile(object sender, RoutedEventArgs e)
    {
        mainWindow.PickPgnFile(PgnFilePicker.Text);
    }
}
