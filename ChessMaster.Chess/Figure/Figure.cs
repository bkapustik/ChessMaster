using ChessMaster.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess.Figure
{
    public class Figure : MoveableEntity
    {
        public readonly Color Color;

        public Figure(Color color)
        { 
            this.Color = color;
        }
    }
}
