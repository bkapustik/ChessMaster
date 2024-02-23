using System.Numerics;

namespace ChessMaster.Space.Coordinations;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Vector3 vector3) => new Vector2(vector3.X, vector3.Y);

    public static Vector3 ToVector3(this Vector2 vector2) => new Vector3(vector2.X, vector2.Y, 0);
}
