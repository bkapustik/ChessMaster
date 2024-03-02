namespace ChessMaster.Space.Coordinations
{
    public struct SpacePosition
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public SpacePosition()
        { 
        
        }

        public SpacePosition(int row, int column)
        { 
            Row = row;
            Column = column;
        }

        public static SpacePosition operator +(SpacePosition a, SpacePosition b) => new SpacePosition(a.Row + b.Row, a.Column + b.Column);
        public static SpacePosition operator -(SpacePosition a, SpacePosition b) => new SpacePosition(a.Row - b.Row, a.Column - b.Column);
        public static SpacePosition AbsDifference(SpacePosition a, SpacePosition b) => new SpacePosition(Math.Abs(a.Row - b.Row), Math.Abs(a.Column - b.Column));
        public static SpacePosition operator +(SpacePosition a, int padding) => new SpacePosition(a.Row + padding, a.Column + padding);
        public static SpacePosition operator *(SpacePosition a, int b) => new SpacePosition(a.Row * b, a.Column * b);
        public static bool operator ==(SpacePosition a, SpacePosition b) => a.Row == b.Row && a.Column == b.Column;
        public static bool operator !=(SpacePosition a, SpacePosition b) => a.Row != b.Row || a.Column != b.Column;
    }
}
