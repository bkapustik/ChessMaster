using ChessMaster.Chess;
using ChessMaster.Chess.Strategy.MatchReplay;
using ChessMaster.ChessDriver;
using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var robot = new ChessRobot("COM3", new ChessMaster.Space.Space(0,10));

            robot.Initialize();
            robot.Configure();
        }
    }
}