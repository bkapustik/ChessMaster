using ChessTracking.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.Tests;

[TestClass]
public class TrackingTests
{
    [TestMethod]
    public void Test()
    {
        var figure = new Figure(FigureType.Pawn, PlayerColor.Black);

        Assert.IsNotNull(figure);
    }
}