using ChessMaster.Chess;
using ChessMaster.ChessDriver;
using ChessMaster.ChessDriver.Strategy;
using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.Robotic;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var path = "C:\\Users\\asus\\Desktop\\Bakalarka\\ChessMaster\\Data\\Anatoly Karpov_vs_Garry Kasparov_1985.pgn";

            var chessStrategy = new MatchReplayChessStrategy(path);

            var chessRunner = new ChessRunner("COM3");
            chessRunner.Initialize();
            //chessRunner.robot.Move(new System.Numerics.Vector2(100,100));

            //chessRunner.Configure();

            //await chessRunner.Run();
        }
    }
}