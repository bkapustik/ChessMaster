﻿using Emgu.CV.Structure;
using System.Drawing;

namespace ChessTracking.Core.ImageProcessing.PipelineData;

/// <summary>
/// Data generated by plane detection
/// </summary>
public class PlaneTrackingData
{
    public Emgu.CV.Image<Rgb, byte> MaskedColorImageOfTable { get; set; }
    public byte[] CannyDepthData { get; set; }
    public Bitmap ColorBitmap { get; set; }
    public bool[] MaskOfTable { get; set; }
}
