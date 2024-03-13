using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Services.Events;

public delegate void HandDetectedEvent(object? o, HandDetectionEventArgs e);

public class HandDetectionEventArgs : EventArgs
{ }
