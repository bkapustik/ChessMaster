using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Tracking.State;

/// <summary>
/// Description, whether chessboard field contains white/black/none figure
/// </summary>
public enum TrackingFieldState
{
    None = 0,
    White,
    Black
}