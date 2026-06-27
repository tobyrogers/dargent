namespace Dargent.Core.Tools;

public class AgentTools
{
    [Description("Reads the content of a file.")]
    public static string ReadFile(string path)
    {
        if (!File.Exists(path))
            return $"Error: File '{path}' not found.";

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    [Description("Writes content to a file.")]
    public static string WriteFile(string path, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, content);
            return $"Successfully wrote to '{path}'.";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }

    [Description("Runs a command in the terminal and returns the output.")]
    public static string RunTerminalCommand(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{command.Replace("\"", "\"\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return "Error: Failed to start process.";

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error)) return $"Output:\n{output}\nError:\n{error}";

            return output;
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }
}