using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Kinect.Messages
{
    /// <summary>
    /// Describes which way should chessboard location move by one field
    /// </summary>
    enum ChessboardMovement
    {
        Vector1Plus,
        Vector1Minus,
        Vector2Plus,
        Vector2Minus
    }
}
