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
        static async Task Main(string[] args)
        {
            var path = "C:\\Users\\asus\\Desktop\\Bakalarka\\ChessMaster\\Data\\Anatoly Karpov_vs_Garry Kasparov_1985.pgn";

            var chessStrategy = new MatchReplayChessStrategy(path);

            //var chessRunner = new ChessRunner(chessStrategy, "COM3");
            var chessRunner = new ChessRunner(new MockRobot());
            chessRunner.Initialize();
            await chessRunner.InitializeStrategy(chessStrategy);

            //chessRunner.Configure();

            await chessRunner.Run();
        }
    }
}