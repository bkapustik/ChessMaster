using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ChessMaster.ControlApp.Pages;

namespace ChessMaster.ControlApp.Helpers;

public class ControlFactory
{
    private readonly MainWindow mainWindow;
    public ControlFactory(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
    }

    public static Button CreateMenuButton(string content) =>
        new Button
        {
            Margin = new Thickness(5),
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
            Height = 40
        };

    public Button CreateBackToConfigurationButton()
    {
        var backToConfigurationButton = CreateMenuButton("Configure");
        backToConfigurationButton.Click += BackToConfigurationClick;
        return backToConfigurationButton;
    }

    public Button CreateChangeStrategyButton()
    {
        var changeStrategyButton = CreateMenuButton("Change Strategy");
        changeStrategyButton.Click += ChangeStrategyClick;
        return changeStrategyButton;
    }

    public Button CreateContinueInGameButton()
    {
        var continueInGameButton = CreateMenuButton("Continue In Game");
        continueInGameButton.Click += ContinueInGameClick;
        return continueInGameButton;
    }


    private void BackToConfigurationClick(object sender, RoutedEventArgs e)
    {
        mainWindow.BackToConfiguration();
    }

    private void ChangeStrategyClick(object o, RoutedEventArgs e)
    {
        mainWindow.NavigateTo(typeof(SelectStrategyPage));
    }

    private void ContinueInGameClick(object o, RoutedEventArgs e)
    {
        mainWindow.ContinueGame();
    }
}
