using ChessMaster.ControlApp.Pages;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;
using WinRT.Interop;


namespace ChessMaster.ControlApp.Windows;

public sealed partial class KinectWindow : Window
{
    public KinectWindow(Type page)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        Resize(1500,1000);
        NavigateTo(page);
    }

    public void NavigateTo(Type page)
    {
        ContentFrame.Navigate(page, this);
    }

    public void CreateNewWindow(Type pageToOpen)
    {
        var newWindow = new KinectWindow(pageToOpen);
        newWindow.Activate();

        this.Closed += (object o, WindowEventArgs e) => {
            newWindow.Close();
        };
    }

    public void Resize(int windowWidth, int windowHeight)
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        AppWindow.Resize(new SizeInt32(windowWidth, windowHeight));
    }
}
