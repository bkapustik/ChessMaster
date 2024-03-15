using ChessTracking.Core.Tracking.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Services.Events;

public delegate void ChessboardStateChangedEvent(object? o, ChessboardStateChangedEventArgs e);

public class ChessboardStateChangedEventArgs : EventArgs
{ 
    public TrackingState TrackingState { get; set; }
    public ChessboardStateChangedEventArgs(TrackingState trackingState)
    {
        TrackingState = trackingState;
    }
}
