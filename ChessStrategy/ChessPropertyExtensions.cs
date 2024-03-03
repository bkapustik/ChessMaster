using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess;

public static class ChessPropertyExtensions
{
    public static char ToUci(this FigureType figureType)
    {
        switch (figureType)
        {
            case FigureType.Pawn: return 'p';
            case FigureType.Bishop: return 'b';
            case FigureType.Knight: return 'n';
            case FigureType.Rook: return 'r';
            default: return ' ';
        }
    }
}
