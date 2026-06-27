using System.Threading.Tasks;

namespace Dargent.Core;

public class SessionManager
{
    private const string SessionsDirectory = "sessions";

    private static string GetSessionFilePath(string sessionId)
    {
        if (!Directory.Exists(SessionsDirectory))
            Directory.CreateDirectory(SessionsDirectory);

        return Path.Combine(SessionsDirectory, $"{sessionId}.json");
    }

    public static async Task SaveSession(string sessionId, IList<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        var filePath = GetSessionFilePath(sessionId);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(history, options);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public static List<ChatMessage>? LoadSession(string sessionId)
    {
        var filePath = GetSessionFilePath(sessionId);
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ChatMessage>>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading session: {ex.Message}");
            return null;
        }
    }
}