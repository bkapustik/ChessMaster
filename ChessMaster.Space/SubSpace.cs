using ChessMaster.Space.Coordinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Space;

public class SubSpace : Entity
{
    public MoveableEntity? Entity { get; set; }

    public SubSpace(float width)
    {
        this.Width = width;
        this.Length = width;
    }

    public SubSpace(float width, Vector2 center)
    {
        this.Width = width;
        this.Length = width;
        this.Center = center;
    }
}
