using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using System;
using Windows.System;
using ChessMaster.ControlApp.Helpers;
using ChessMaster.ChessDriver.Models;

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

public class Holdable
{
    protected readonly DispatcherTimer timer;
    public long Counter { get; set; }

    public Holdable()
    {
        timer = new DispatcherTimer();
        timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        timer.Tick += IncreaseTimer;
    }

    protected void IncreaseTimer(object sender, object args)
    {
        Counter++;
    }
}

public class HoldableKey : Holdable
{
    protected readonly VirtualKey key;

    public HoldableKey(UIElement element, VirtualKey key) : base()
    { 
        this.key = key; 

        element.KeyDown += ElementKeyDown;
        element.KeyUp += ElementKeyUp;
    }
    protected virtual void ElementKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == key)
        {
            Counter = 0;
            timer.Start();
        }
    }
    protected virtual void ElementKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == key)
        {
            timer.Stop();
        }
    }
}

public class HoldableControl : Holdable
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
        Counter = 0;
        timer.Start();
    }
    protected virtual void ReleasedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        timer.Stop();
    }
}

public class HoldableMoveKey : HoldableKey
{
    private UIGameState positionSetupState;
    private bool hasBeenPressed;

    public HoldableMoveKey(UIElement element, VirtualKey key, UIGameState positionSetupState) : base(element, key)
    { 
        this.positionSetupState = positionSetupState;
    }

    protected override void ElementKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == key)
        {
            if (!hasBeenPressed)
            {
                hasBeenPressed = true;
                Counter = 0;
                timer.Start();
            }
        }
    }

    protected override void ElementKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == key)
        {
            hasBeenPressed = false;
            timer.Stop();

            if (!MoveHelper.CanMove(positionSetupState.RobotState, positionSetupState.MovementState))
            {
                return;
            }

            if (Counter <= 10)
            {
                Counter = 10;
            }

            positionSetupState.DesiredPosition = MoveHelper.ChangeDesiredPosition(key, Counter, positionSetupState);
        }
    }
}

public class HoldableMoveButton : HoldableControl
{
    private UIGameState positionSetupState;

    public HoldableMoveButton(Button button, UIGameState positionSetupState) :
        base(button)
    {
        this.positionSetupState = positionSetupState;
    }

    protected override void ReleasedMoveButton(object sender, PointerRoutedEventArgs e)
    {
        timer.Stop();

        if (!MoveHelper.CanMove(positionSetupState.RobotState, positionSetupState.MovementState))
        {
            return;
        }

        if (Counter <= 10)
        {
            Counter = 10;
        }

        positionSetupState.DesiredPosition = MoveHelper.ChangeDesiredPosition(button.Name, Counter, positionSetupState);
    }
}