using ChessMaster.ChessDriver.ChessStrategy.MatchReplayStrategy;
using ChessMaster.ChessDriver.Strategy;

namespace ChessMaster.ChessDriver.ChessStrategy;

public abstract class ChessStrategyFacade
{
    public abstract string Name { get; }
    public abstract bool NeedsConfiguration { get; }
    public abstract void Configure(string configuration);
    public abstract IChessStrategy CreateStrategy();
}

public class PgnStrategyFacade : ChessStrategyFacade
{
    private string file;
    public override string Name { get => "Replay Match"; }
    public override IChessStrategy CreateStrategy() => new MatchReplayChessStrategy(file);
    public override bool NeedsConfiguration => true;
    public override void Configure(string configuration)
    {
        file = configuration;
    }
}

public class MockPgnStrategyFacade : ChessStrategyFacade
{
    private byte[] data = Properties.Resources.Anatoly_Karpov_vs_Garry_Kasparov_1985;
    public override string Name { get => "MOCK Replay Match"; }
    public override IChessStrategy CreateStrategy() => new MockMatchReplayChessStrategy(data);
    public override bool NeedsConfiguration => false;
    public override void Configure(string configuration)
    {
    }
}