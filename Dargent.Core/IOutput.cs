using System.Threading.Tasks;

namespace Dargent.Core;

public enum OutputType
{
    None,
    Normal,
    Error,
    Warning,
}

public interface IOutput
{
    Task Spinner(string status, Func<Task> action);
    
    void Write(string text, OutputType outputType = OutputType.Normal);

    Task<bool> Approve(string text, CancellationToken cancellationToken = default);

    Task<string> Prompt(string prompt, CancellationToken cancellationToken = default);

    void DrawSeparator(string usage);
}

public static class OutputExtensions
{
    public static void WriteNormal(this IOutput output, string text)
    {
        output.Write(text, OutputType.Normal);
    }

    public static void WriteError(this IOutput output, string text)
    {
        output.Write(text, OutputType.Error);
    }

    public static void WriteWarning(this IOutput output, string text)
    {
        output.Write(text, OutputType.Warning);
    }
}