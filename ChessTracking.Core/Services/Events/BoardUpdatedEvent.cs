using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Services.Events;

public delegate void BoardUpdatedEvent(object? o, BoardUpdatedEventArgs e);

public class BoardUpdatedEventArgs : EventArgs
{
    public Bitmap Bitmap { get; set; }
    public BoardUpdatedEventArgs(Bitmap bitmap)
    {
        Bitmap = bitmap;
    }
}
