using System.Drawing;

namespace ChessTracking.Core.Services.Events;

public delegate void VizualizationUpdatedEvent(object? o, VizualizationUpdateEventArgs e);

public class VizualizationUpdateEventArgs : EventArgs
{
    public Bitmap Bitmap { get; set; }

    public VizualizationUpdateEventArgs(Bitmap bitmap)
    {
        Bitmap = bitmap;
    }
}