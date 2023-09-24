using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space
{
    public class Space : Entity
    {
        public SubSpace[,] SubSpaces;

        public Space(int width, int height) 
        {
            this.SubSpaces = new SubSpace[width, height];
        }

        public Space(float width, float height)
        { 
            
        }
    }
}
