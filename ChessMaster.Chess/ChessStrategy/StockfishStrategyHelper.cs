using ChessMaster.Chess;
using ChessMaster.ChessDriver.ChessMoves;
using ChessMaster.ChessDriver.Models;
using ChessMaster.Space.Coordinations;

namespace ChessMaster.ChessDriver.ChessStrategy;

public static class StockfishStrategyHelper
{
    public static ChessMove CreateMoveFromUci(string uci, ChessBoardGeneral chessBoard)
    {
        var source = new SpacePosition(GetRow(uci[1]) - 1, GetColumn(uci[0]));
        var target = new SpacePosition(GetRow(uci[3]) - 1, GetColumn(uci[2]));


        if (uci.Length == 5)
        {
            var promotionFigure = GetPawnPromotionFigure(uci[4]);

            chessBoard.Grid[target.Row, target.Column] = chessBoard.Grid[source.Row, source.Column];
            chessBoard.Grid[target.Row, target.Column]!.FigureType = promotionFigure;
            chessBoard.Grid[source.Row, source.Column] = null;

            return new PawnPromotionMove(source, target, promotionFigure, false, uci);
        }

        if (chessBoard.Grid[target.Row, target.Column] != null)
        {
            chessBoard.Grid[target.Row, target.Column] = chessBoard.Grid[source.Row, source.Column];
            chessBoard.Grid[source.Row, source.Column] = null;

            return new CaptureMove(source, target, false, uci);
        }

        if (chessBoard.Grid[source.Row, source.Column]?.FigureType == FigureType.King
            && Math.Abs(source.Column - target.Column) == 2
            && source.Row == target.Row)
        {
            var castlingMove = GetCastlingMove(source, target, uci);

            chessBoard.Grid[target.Row, target.Column] = chessBoard.Grid[source.Row, source.Column];
            chessBoard.Grid[source.Row, source.Column] = null;

            chessBoard.Grid[castlingMove.RookTarget.Row, castlingMove.RookTarget.Column] = chessBoard.Grid[castlingMove.RookSource.Row, castlingMove.RookSource.Column];
            chessBoard.Grid[castlingMove.RookSource.Row, castlingMove.RookSource.Column] = null;

            return new CastlingMove(castlingMove, false, uci);
        }

        chessBoard.Grid[target.Row, target.Column] = chessBoard.Grid[source.Row, source.Column];
        chessBoard.Grid[source.Row, source.Column] = null;

        return new ChessMove(source, target, false, uci);
    }

    public static int GetColumn(char col) => col - 'a';
    public static int GetRow(char row) => row - '0';
    public static FigureType GetPawnPromotionFigure(char castlingFigure)
    {
        switch (castlingFigure)
        {
            case 'R':
            case 'r': return FigureType.Rook;
            case 'N':
            case 'n': return FigureType.Knight;
            case 'B':
            case 'b': return FigureType.Bishop;
            case 'Q':
            case 'q': return FigureType.Queen;
            default: throw new InvalidOperationException($"Can not create Uci castling figure type from '{castlingFigure}'");
        }
    }
    public static Castling GetCastlingMove(SpacePosition source, SpacePosition target, string uci)
    {
        if (source.Column != 4)
        {
            throw new InvalidOperationException("Invalid castling move: King does not start from the standard column.");
        }

        if (source.Row != 0 && source.Row != 7)
        {
            throw new InvalidOperationException("Invalid castling move: King is not on a valid row.");
        }

        bool isKingside = target.Column == 6; 
        bool isQueenside = target.Column == 2;

        if (!isKingside && !isQueenside)
        {
            throw new InvalidOperationException("Invalid castling move: Target column does not match any valid castling move.");
        }

        if (isKingside)
        {
            return new Castling
            {
                KingSource = source,
                KingTarget = target,
                RookSource = new SpacePosition(source.Row, 7),
                RookTarget = new SpacePosition(source.Row, 5) 
            };
        }
        else
        {
            return new Castling
            {
                KingSource = source,
                KingTarget = target,
                RookSource = new SpacePosition(source.Row, 0),
                RookTarget = new SpacePosition(source.Row, 3) 
            };
        }
    }
}
