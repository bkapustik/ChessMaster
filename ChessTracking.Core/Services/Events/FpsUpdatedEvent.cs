using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Services.Events;

public delegate void FpsUpdatedEvent(object? o, FpsUpdatedEventArgs e);

public class FpsUpdatedEventArgs : EventArgs
{
    public int FpsValue { get; set; }
    public FpsUpdatedEventArgs(int fpsValue)
    {
        FpsValue = fpsValue;
    }
}
