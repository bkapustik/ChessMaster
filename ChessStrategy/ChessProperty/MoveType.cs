using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess.Property
{
    public enum MoveType
    {
        Default,
        Capture,
        KingCastling,
        QueenSideCastling,
        PawnPromotion
    }
}
