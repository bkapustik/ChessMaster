using ChessMaster.Space.Coordinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Chess.Strategy.MatchReplay;

public class MatchReplayChessStrategy : IChessStrategy
{
    public MatchReplayChessStrategy()
    {

    }

    public async Task Initialize()
    {

    }

    public async Task<Move> GetNextMove()
    {
        return new Move();
    }
}
