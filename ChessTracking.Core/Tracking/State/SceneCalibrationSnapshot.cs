using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Tracking.State;

/// <summary>
/// Class carying visual info from calibration to user for better visual feedback what happened
/// </summary>
public class SceneCalibrationSnapshot
{
    public Bitmap MaskedColorImage { get; set; }
    public Bitmap GrayImage { get; set; }
    public Bitmap BinarizationImage { get; set; }
    public Bitmap CannyImage { get; set; }
}
