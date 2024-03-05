using ChessMaster.Chess;
using ChessMaster.Chess.Property;
using ChessMaster.Space.Coordinations;
using ChessMaster.ChessDriver.Strategy;
using NuGet.Frameworks;

namespace ChessMaster.Tests
{
    [TestClass]
    public class PgnFileParserTests
    {
        [TestMethod]
        public void PawnFigureNotSpecified()
        {
            var result = ChessFileParser.ParseMove("e4");
            Assert.AreEqual(result.Target, new SpacePosition(3, 4));
            Assert.AreEqual(result.Figure, FigureType.Pawn);

            Assert.AreEqual(ChessFileParser.ParseMove("ae1").Figure, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("a1e1").Figure, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("axe1").Figure, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("axe1#").Figure, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("ae1+").Figure, FigureType.Pawn);
        }

        [TestMethod]
        public void FigureSpecified()
        {
            Assert.AreEqual(ChessFileParser.ParseMove("Pe1").Figure, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("Qe1").Figure, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("Ke1").Figure, FigureType.King);
            Assert.AreEqual(ChessFileParser.ParseMove("Re1").Figure, FigureType.Rook);
            Assert.AreEqual(ChessFileParser.ParseMove("Ne1").Figure, FigureType.Knight);
            Assert.AreEqual(ChessFileParser.ParseMove("Be1").Figure, FigureType.Bishop);

            Assert.AreEqual(ChessFileParser.ParseMove("Bae1").Figure, FigureType.Bishop);
            Assert.AreEqual(ChessFileParser.ParseMove("Ba1e1").Figure, FigureType.Bishop);
            Assert.AreEqual(ChessFileParser.ParseMove("Baxe1").Figure, FigureType.Bishop);
            Assert.AreEqual(ChessFileParser.ParseMove("Baxe1#").Figure, FigureType.Bishop);
            Assert.AreEqual(ChessFileParser.ParseMove("Bae1+").Figure, FigureType.Bishop);
        }

        [TestMethod]
        public void Castling()
        {
            Assert.AreEqual(ChessFileParser.ParseMove("O-O").MoveType, MoveType.KingCastling);
            Assert.AreEqual(ChessFileParser.ParseMove("O-O-O").MoveType, MoveType.QueenSideCastling);

            Assert.AreEqual(ChessFileParser.ParseMove("Pe1").MoveType, MoveType.Default);
        }

        [TestMethod]
        public void MoveTypeTest()
        {
            Assert.AreEqual(ChessFileParser.ParseMove("Bae1").MoveType, MoveType.Default);
            Assert.AreEqual(ChessFileParser.ParseMove("Ba1e1").MoveType, MoveType.Default);
            Assert.AreEqual(ChessFileParser.ParseMove("Baxe1").MoveType, MoveType.Capture);
            Assert.AreEqual(ChessFileParser.ParseMove("Baxe1#").MoveType, MoveType.Capture);
            Assert.AreEqual(ChessFileParser.ParseMove("Bae1+").MoveType, MoveType.Default);
            Assert.AreEqual(ChessFileParser.ParseMove("ae1").MoveType, MoveType.Default);
            Assert.AreEqual(ChessFileParser.ParseMove("a1e1").MoveType, MoveType.Default);
            Assert.AreEqual(ChessFileParser.ParseMove("axe1").MoveType, MoveType.Capture);
            Assert.AreEqual(ChessFileParser.ParseMove("axe1#").MoveType, MoveType.Capture);
            Assert.AreEqual(ChessFileParser.ParseMove("ae1+").MoveType, MoveType.Default);

            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q#").MoveType, MoveType.PawnPromotion);
        }

        [TestMethod]
        public void PawnPromotion()
        {
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q#").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=Q#").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("Pee8=Q#").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe1e8=Q#").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=P#").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q").MoveType, MoveType.PawnPromotion);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q+").MoveType, MoveType.PawnPromotion);

            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q#").PawnPromotion?.FigureType, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=Q#").PawnPromotion?.FigureType, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("Pee8=Q#").PawnPromotion?.FigureType, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe1e8=Q#").PawnPromotion?.FigureType, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=P#").PawnPromotion?.FigureType, FigureType.Pawn);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q").PawnPromotion?.FigureType, FigureType.Queen);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q+").PawnPromotion?.FigureType, FigureType.Queen);

            Assert.AreEqual(ChessFileParser.ParseMove("e8=K#").PawnPromotion?.FigureType, FigureType.King);
        }

        [TestMethod]
        public void Target()
        {
            var e8Position = new SpacePosition(7, 4);

            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q#").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=Q#").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("Pee8=Q#").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe1e8=Q#").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=P#").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q").Target, e8Position);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q+").Target, e8Position);
        }

        [TestMethod]
        public void Source()
        {
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q#").Source, null);
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=Q#").Source, null);
            Assert.AreEqual(ChessFileParser.ParseMove("Pee8=Q#").Source, new SpacePosition(default, 4));
            Assert.AreEqual(ChessFileParser.ParseMove("Pe1e8=Q#").Source, new SpacePosition(0, 4));
            Assert.AreEqual(ChessFileParser.ParseMove("Pe8=P#").Source, null);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q").Source, null);
            Assert.AreEqual(ChessFileParser.ParseMove("e8=Q+").Source, null);
        }
    }
}