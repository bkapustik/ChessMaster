using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space.Coordinations
{
    public class Movement
    {
        public SpacePosition Source { get; set; }
        public SpacePosition Target { get; set; }

        public Movement(SpacePosition source, SpacePosition target)
        { 
            Source = source;
            Target = target;
        }
    }
}
