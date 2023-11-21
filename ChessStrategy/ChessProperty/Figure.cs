using ChessMaster.Chess;
using ChessMaster.Space;
using ChessMaster.Space.Coordinations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;

namespace ChessMaster.Chess
{
    public interface IChessFigure
    {
        public ChessColor ChessColor { get; set; }
        public FigureType FigureType { get; set; }
    }
}
