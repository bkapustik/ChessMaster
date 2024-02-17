using ChessMaster.Chess;
using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.Strategy;

public struct PgnMove
{
    public FigureType Figure { get; set; }
    public ChessColor Color { get; set; }
    public CheckingMove CheckingMove { get; set; }
    public MoveType MoveType { get; set; }
    public PawnPromotion? PawnPromotion { get; set; }
    public SpacePosition? Source { get; set; }
    public SpacePosition? Target { get; set; }
    public bool IsEnfOfGame { get; set; }
    public string Message { get; set; } = "";

    public PgnMove()
    {
        
    }

    public PgnMove(string message)
    { 
        Message = message;
    }
}
