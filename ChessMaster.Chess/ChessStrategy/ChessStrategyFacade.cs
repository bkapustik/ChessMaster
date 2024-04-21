using ChessMaster.ChessDriver.ChessStrategy.MatchReplayStrategy;
using ChessMaster.ChessDriver.ChessStrategy.StockFishKinectTrackingStrategy;
using ChessMaster.ChessDriver.Services;
using ChessMaster.ChessDriver.Strategy;

namespace ChessMaster.ChessDriver.ChessStrategy;

public abstract class ChessStrategyFacade
{
    public bool ContinueWithOldContext = false;
    public virtual string FilePickerText { get; } = "";
    public abstract string Name { get; }
    public abstract bool NeedsFileConfiguration { get; }
    public abstract bool NeedsKinectConfiguration { get; }
    public abstract void Configure(string configuration);
    public abstract void Configure(IKinectService kinectService);
    public virtual List<string> AcceptedFileTypes { get; } = new List<string>();
    public abstract IChessStrategy CreateStrategy();
    public abstract bool CanAcceptOldContext { get; }
}

public class PgnStrategyFacade : ChessStrategyFacade
{
    private string file;
    public override string Name { get => "Replay Match"; }
    public override IChessStrategy CreateStrategy() => new MatchReplayChessStrategy(file);
    public override List<string> AcceptedFileTypes => new List<string> { ".pgn" };
    public override bool NeedsFileConfiguration => true;
    public override bool NeedsKinectConfiguration => false;
    public override bool CanAcceptOldContext => false;
    public override string FilePickerText => "Please select a text file containing chess game record in PGN notation";
    public override void Configure(string configuration)
    {
        file = configuration;
    }
    public override void Configure(IKinectService kinectService)
    { 
        
    }
}

public class MockPgnStrategyFacade : ChessStrategyFacade
{
    private byte[] data = Properties.Resources.Anatoly_Karpov_vs_Garry_Kasparov_1985;
    public override string Name { get => "MOCK Replay Match"; }
    public override IChessStrategy CreateStrategy() => new MockMatchReplayChessStrategy(data);
    public override bool NeedsFileConfiguration => false;
    public override bool NeedsKinectConfiguration => false;
    public override bool CanAcceptOldContext => false;
    public override List<string> AcceptedFileTypes => new List<string> { ".pgn" };
    public override void Configure(string configuration)
    {

    }
    public override void Configure(IKinectService kinectService)
    {
        
    }
}

public class StockFishStrategyFacade : ChessStrategyFacade
{
    private string? File;
    public override string Name { get => "Watch AI Match"; }
    public override IChessStrategy CreateStrategy() => new StockfishAgainstStockfishStrategy(File!);
    public override bool NeedsFileConfiguration => true;
    public override bool NeedsKinectConfiguration => false;
    public override bool CanAcceptOldContext => true;
    public override List<string> AcceptedFileTypes => new List<string> { ".exe" };
    public override string FilePickerText => "Please select a stockfish.exe file";
    public override void Configure(string configuration)
    {
        File = configuration;
    }

    public override void Configure(IKinectService kinectService)
    {
        
    }
}

public class StockFishKinectStrategyFacade : ChessStrategyFacade
{
    private IKinectService? KinectService { get; set; }
    private string? File;
    public override string Name { get => "Play against AI";  }
    public override IChessStrategy CreateStrategy() => new StockfishKinectChessTrackingStrategy(KinectService!, File!);
    public override bool NeedsFileConfiguration => true;
    public override bool NeedsKinectConfiguration => true;
    public override bool CanAcceptOldContext => true;
    public override List<string> AcceptedFileTypes => new List<string> { ".exe" };
    public override string FilePickerText => "Please select a stockfish.exe file";
    public override void Configure(string configuration)
    {
        File = configuration;
    }
    public override void Configure(IKinectService kinectService)
    {
        KinectService = kinectService;
    }
}