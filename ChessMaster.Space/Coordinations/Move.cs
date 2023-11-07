using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space.Coordinations
{
    public struct Move
    {
        public SpacePosition Source { get; set; }
        public SpacePosition Target { get; set; }

        public Move(SpacePosition source, SpacePosition target)
        {
            Source = source;
            Target = target;
        }
    }
}
