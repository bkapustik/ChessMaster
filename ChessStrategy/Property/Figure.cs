using ChessMaster.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess
{
    public class Figure : MoveableEntity
    {
        public readonly ChessColor Color;
        public readonly FigureType Type;

        public Figure(ChessColor color)
        { 
            this.Color = color;
        }
    }
}
