using ChessMaster.ControlApp.Helpers;
using ChessMaster.ControlApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class FilePickerPage : Page
{
    private MainWindow mainWindow;
    private readonly ConfigurationService configurationService;
    private List<string> AcceptedFileTypes;
    public FilePickerPage()
    {
        this.InitializeComponent();
        configurationService = App.Services.GetRequiredService<ConfigurationService>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        mainWindow = App.MainWindow;
        var controlFactory = new ControlFactory(mainWindow);

        mainWindow.AddMenuButton(controlFactory.CreateBackToConfigurationButton());
        mainWindow.AddMenuButton(controlFactory.CreateChangeStrategyButton());

        AcceptedFileTypes = configurationService.AcceptedFileTypes;
    }

    private void SelectFileButtonClicked(object sender, RoutedEventArgs e)
    {
        FileOpenPicker filePicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail
        };

        foreach (var fileType in AcceptedFileTypes)
        {
            filePicker.FileTypeFilter.Add(fileType);
        }

        var windowHandle = WindowNative.GetWindowHandle(mainWindow);

        InitializeWithWindow.Initialize(filePicker, windowHandle);

        Task.Run(async () =>
        {
            var selectedFile = await filePicker.PickSingleFileAsync();

            if (selectedFile is not null && !string.IsNullOrEmpty(selectedFile.Path))
            {
                mainWindow.DispatcherQueue.TryEnqueue(() => FilePicker.Text = selectedFile.Path);
            }
        });
    }

    private void ConfirmFile(object sender, RoutedEventArgs e)
    {
        mainWindow.PickFile(FilePicker.Text);
    }
}
