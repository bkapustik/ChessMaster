using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using System;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using ChessMaster.ControlApp.Services;

namespace ChessMaster.ControlApp;

public delegate void HoldableControlEventHandler(object sender, HoldableControlEventArgs args);

public class HoldableControlEventArgs : EventArgs
{
    public long Counter { get; set; }

    public HoldableControlEventArgs(long counter)
    {
        Counter = counter;
    }
}

public abstract class Holdable
{
    protected readonly DispatcherTimer timer;
    protected bool isTimerRunning = false;

    public Holdable()
    {
        timer = new DispatcherTimer();
        timer.Interval = new TimeSpan(0, 0, 0, 0, (int)IConfigurationService.TICK_SPEED);
        timer.Tick += (sender, args) =>
        {
            if (!isTimerRunning)
            {
                return;
            }
            DoWork(sender, args);
        };
    }

    protected abstract void DoWork(object sender, object args);
}

public abstract class HoldableKey : Holdable
{
    protected readonly VirtualKey key;

    public HoldableKey(UIElement element, VirtualKey key) : base()
    { 
        this.key = key;

        element.KeyDown += (sender, e) =>
        {
            if (e.Key == key && !isTimerRunning)
            {
                isTimerRunning = true;
                timer.Start();
            }
        };
        element.KeyUp += (sender, e) =>
        {
            if (e.Key == key)
            {
                isTimerRunning = false;
                timer.Stop();
            }
        };
    }
}

public abstract class HoldableControl : Holdable
{
    protected Button button;

    public HoldableControl(Button button) : base()
    {
        this.button = button;

        this.button.AddHandler(Button.PointerPressedEvent, new PointerEventHandler(PressedMoveButton), true);
        this.button.AddHandler(Button.PointerReleasedEvent, new PointerEventHandler(ReleasedMoveButton), true);
    }

    protected virtual void PressedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            timer.Start();
        }
    }
    protected virtual void ReleasedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        isTimerRunning = false;
        timer.Stop();
    }
}

public class HoldableMoveKey : HoldableKey
{
    private readonly IUIRobotService robotService;
    private readonly IConfigurationService configurationService;

    public HoldableMoveKey(UIElement element, VirtualKey key) : base(element, key)
    { 
        this.robotService = App.Services.GetRequiredService<IUIRobotService>();
        this.configurationService = App.Services.GetRequiredService<IConfigurationService>();
    }

    protected override void DoWork(object sender, object args)
    {
        configurationService.RobotDesiredPosition = configurationService.ControlDesiredPosition(key);
    }
}

public class HoldableMoveButton : HoldableControl
{

    private readonly IUIRobotService robotService;
    private readonly IConfigurationService configurationService;

    public HoldableMoveButton(Button button) :
        base(button)
    {
        robotService = App.Services.GetRequiredService<IUIRobotService>();
        this.configurationService = App.Services.GetRequiredService<IConfigurationService>();
    }

    protected override void DoWork(object sender, object args)
    {
        configurationService.RobotDesiredPosition = configurationService.ControlDesiredPosition(button.Name);
    }
}