﻿using System;
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

    public GameData(ChessboardModel chessboard, PlayerColor playerOnMove, GameState endState)
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
