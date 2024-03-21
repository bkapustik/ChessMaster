using System.Drawing;
using System.Reflection;

namespace ChessTracking.Core.Game;

public static class GameRenderer
{
    private static Bitmap ChessboardBitmap { get; set; }

    static GameRenderer()
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Ensure that PropertiesAccessor.GetResourceBitmap correctly loads the chessboard image
        ChessboardBitmap = PropertiesAccessor.GetResourceBitmap("Chessboard", assembly);
    }

    public static Bitmap RenderGameState(GameData game)
    {
        // Create a new bitmap with the same dimensions as the original chessboard
        var bm = new Bitmap(ChessboardBitmap.Width, ChessboardBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        using (var graphics = Graphics.FromImage(bm))
        {
            // First, draw the original chessboard onto the new bitmap
            graphics.DrawImage(ChessboardBitmap, 0, 0, ChessboardBitmap.Width, ChessboardBitmap.Height);

            // Then, draw each chess piece on the board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    // inverting y coordinate because of difference in chess and bitmap coordinates
                    var figure = game.Chessboard.Figures[x, 7 - y];
                    if (figure != null)
                    {
                        var figureBitmap = Figure.GetBitmapRepresentation(figure.Type, figure.Color);
                        graphics.DrawImage(figureBitmap, x * 140, y * 140, 140, 140);
                    }
                }
            }
        }

        return bm;
    }
}
