using System.Drawing;
using System.Reflection;

namespace ChessTracking.Core;

public static class PropertiesAccessor
{
    public static Bitmap GetResourceBitmap(string resourceName, Assembly assembly)
    {
        const string prefix = "ChessTracking.Core.Game.Images.";
        const string sufix = ".png";

        resourceName = prefix + resourceName + sufix;

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) return null;
            return new Bitmap(stream);
        }
    }
}
