using ChessMaster.Chess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.ChessDriver
{
    public interface IHeightProvider
    {
        float GetHeight(FigureType figureType);
    }
    public class HeightProvider
    {
        public float GetHeight(FigureType figureType) 
        {
            switch (figureType)
            {
                case FigureType.Rook:
                    return 25;
                case FigureType.Knight:
                    return 30;
                case FigureType.Bishop:
                    return 22;
                case FigureType.King:
                    return 50;
                case FigureType.Queen:
                    return 50;
                default:
                    return 18;
            }
        }
    }
}
