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