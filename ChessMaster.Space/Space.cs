namespace ChessMaster.Space
{
    public class Space : Entity
    {
        public SubSpace[,] SubSpaces { get; set; }
        public readonly int width;
        public readonly int height;

        public Space(int width, int height) 
        {
            this.width = width;
            this.height = height;
            this.SubSpaces = new SubSpace[width, height];
        }

        public Space(float width, float height)
        { 
            
        }
    }
}
