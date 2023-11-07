using ChessMaster.Chess.Strategy.MatchReplay;

namespace ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            string move = "e4";

            var result = ChessFileParser.ParseMove(move);

            Console.WriteLine(result);
        }
    }
}