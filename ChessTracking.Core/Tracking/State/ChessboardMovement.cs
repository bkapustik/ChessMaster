﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Tracking.State;

/// <summary>
/// Describes which way should chessboard location move by one field
/// </summary>
public enum ChessboardMovement
{
    Vector1Plus,
    Vector1Minus,
    Vector2Plus,
    Vector2Minus
}
