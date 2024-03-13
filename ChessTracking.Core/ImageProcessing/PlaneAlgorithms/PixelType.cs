using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.ImageProcessing.PlaneAlgorithms;

/// <summary>
/// Additional flag for custom camera space point implementation
/// </summary>
public enum PixelType
{
    NotMarked,
    Invalid,
    Table,
    Object
}
