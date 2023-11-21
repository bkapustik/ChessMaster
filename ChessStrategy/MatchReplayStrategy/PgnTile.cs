using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess.Strategy.MatchReplay;

public class PgnTile
{
    public ChessColor ChessColor { get; set; }
    public PgnFigure? Figure { get; set; }
}
