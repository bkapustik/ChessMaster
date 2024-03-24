using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Game;

[Serializable]
public class GameData
{
    public ChessboardModel Chessboard;
    public PlayerColor PlayerOnMove { get; set; }
    public GameState EndState { get; set; }
    public List<string> RecordOfGame { get; set; }

    public List<GameMove> Moves { get; set; }

    public GameData(GameData gameData)
    {
        Chessboard = new ChessboardModel(gameData.Chessboard);
        PlayerOnMove = gameData.PlayerOnMove;
        EndState = gameData.EndState;
        Moves = new List<GameMove>();
        for (int i = 0; i < gameData.Moves.Count(); i++)
        {
            Moves.Add(new GameMove(gameData.Moves[i]));
        }
        RecordOfGame = gameData.RecordOfGame.ToList();
    }
    public GameData(ChessboardModel chessboard, PlayerColor playerOnMove, GameState endState)
    {
        Chessboard = chessboard;
        PlayerOnMove = playerOnMove;
        EndState = endState;
        RecordOfGame = new List<string>();
        Moves = new List<GameMove>();
    }

    public GameData(ChessboardModel chessboard, PlayerColor playerOnMove, GameState endState, List<string> recordOfGame, List<GameMove> moves)
    {
        Chessboard = chessboard;
        PlayerOnMove = playerOnMove;
        EndState = endState;
        RecordOfGame = new List<string>();
        Moves = new List<GameMove>();
    }

    public string ExportGameToAlgebraicNotation()
    {
        var acumulator = new StringBuilder("");

        foreach (var record in RecordOfGame)
        {
            acumulator.Append(record + Environment.NewLine);
        }

        return acumulator.ToString();
    }

}
