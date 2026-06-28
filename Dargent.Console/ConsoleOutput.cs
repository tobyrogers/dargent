using System.Collections.Generic;
using System.Threading;
using Dargent.Core;

namespace Dargent.Console;

public class ConsoleOutput : IOutput
{
    public void Write(string text)
    {
        AnsiConsole.WriteLine(text);
    }

    public async Task Stream(IAsyncEnumerable<string> stream)
    {
        await foreach (var line in stream)
        {
            AnsiConsole.Write(line);
        }
    }

    public bool Approve(string text)
    {
        return AnsiConsole.Confirm(text);
    }

    public async Task<string> Prompt(CancellationToken cancellationToken )
    {
        return await AnsiConsole.AskAsync<string>("User >", cancellationToken);

    }
}