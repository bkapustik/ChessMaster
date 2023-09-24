using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class Field
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Index;
        public readonly string Name;

        public Field(int x, int y)
        {
            X = x;
            Y = y;
            Index = y * 8 + x;
            Name = (char)((int)'a' + x) + (y+1).ToString();
        }
    }

    public enum PieceType
    {
        Pawn,
        Bishop,
        Knight,
        Rook,
        King,
        Queen,
    }

    public enum PieceColor
    {
        White,
        Black,
    }

    public class Piece
    {
        private Field location = null;
        private PieceType type;
        public readonly PieceColor Color;
        
        public PieceType Type { get { return type; } }
        public Field Location { get { return location; } }


        public Piece(PieceType type, PieceColor color)
        {
            this.type = type;
            Color = color;
        }

        public void Move(Field to)
        {
            location = to;
        }
    }

    public class Board
    {
        private readonly Field[] fields = new Field[64];
        private readonly Field[,] fieldsIndex = new Field[8, 8];

        private readonly Piece[] pieces;
        private readonly Piece[,] piecesIndex = new Piece[8, 8];
        private readonly Dictionary<PieceColor, Dictionary<PieceType, Piece[]>> piecesTyped = new Dictionary<PieceColor, Dictionary<PieceType, Piece[]>>();
        private static readonly Dictionary<PieceType, int> pieceTypeCounts = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 },
            { PieceType.Queen, 1 },
            { PieceType.Bishop, 2 },
            { PieceType.Knight, 2 },
            { PieceType.Rook, 2 },
            { PieceType.Pawn, 2 },
        };

        private void createPieces(List<Piece> accumulator, PieceColor color)
        {
            foreach (var typeCount in pieceTypeCounts)
            {
                piecesTyped[color][typeCount.Key] = new Piece[typeCount.Value];
                for (int i = 0; i < typeCount.Value; ++i)
                {
                    var piece = new Piece(typeCount.Key, color);
                    piecesTyped[color][typeCount.Key][i] = piece;
                    accumulator.Add(piece);
                }
            }
        }

        public Board()
        {
            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x) {
                    var field = new Field(x, y);
                    fields[y * y + x] = field;
                    fieldsIndex[x,y] = field;
                }
            }

            var pieces = new List<Piece>();
            createPieces(pieces, PieceColor.White);
            createPieces(pieces, PieceColor.Black);
            this.pieces = pieces.ToArray();
        }


        public void Move(Piece piece, Field to)
        {
            piece.Move(to);
        }

        public void StartingPosition()
        {
            foreach (var piece in pieces)
            {
                piece.Move(null);
            }

            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x)
                {
                    piecesIndex[x, y] = null;
                }
            }
        }
    }
}
