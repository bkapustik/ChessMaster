using ChessMaster.ChessDriver;
using ChessMaster.Space.Coordinations;
using System.Numerics;
using System.Threading.Tasks;

namespace ChessMaster.ControlApp.Models;

public class CornerPosition
{
    private readonly ChessRunner chessRunner;
    public bool Locked { get; set; } = false;
    public Vector2 Position { get; set; }

    public CornerPosition() => chessRunner = ChessRunner.Instance;

    public string GetPositionString() => $"{Position.X}, {Position.Y}";

    /// <summary>
    /// 
    /// </summary>
    /// <returns>resulting <see cref="LockState"/></returns>
    public LockState ChangeLock()
    {
        if (Locked)
        {
            Locked = false;
            return LockState.Unlocked;
        }

        Task.Run(() =>
        {
            var state = chessRunner.GetRobotState();
            Locked = true;
            Position = state.Position.ToVector2();
        });

        return LockState.Locked;
    }
}

public enum LockState
{
    Locked,
    Unlocked
}