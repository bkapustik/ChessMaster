using System.Numerics;

namespace ChessMaster.ControlApp.Extensions;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Vector3 vector3) => new Vector2(vector3.X, vector3.Y);
}