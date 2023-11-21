using ChessMaster.Space.Coordinations;

namespace ChessMaster.Space
{
    public class Space : Entity
    {
        public SubSpace[,] SubSpaces { get; set; }
        public SpacePosition Origin { get; set; }

        public Space(int width) 
        {
            this.SubSpaces = new SubSpace[width, width];
            Width = width;
            Length = width;
        }

        public Space(int width, int length)
        {
            this.SubSpaces = new SubSpace[length, width];
        }
    }
}
