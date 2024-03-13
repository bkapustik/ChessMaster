using System.Text.Json;

namespace ChessTracking.Core.Utils;

public static class CloneExtensions
{
    public static T DeepClone<T>(this T source)
    {
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
        var jsonString = JsonSerializer.Serialize(source, options);
        return JsonSerializer.Deserialize<T>(jsonString, options);
    }
}