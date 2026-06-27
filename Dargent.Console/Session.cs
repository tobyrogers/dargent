using System.Text;
using Dargent.Core;

namespace Dargent.Console;

public class Session(AgentSessionFactory agentSessionFactory)
{
    public async Task RunAsync()
    {
        var agentSession = agentSessionFactory.CreateSession();

        AnsiConsole.WriteLine("Argent session started\nType 'exit' to quit, or 'save' to force save the session.");

        while (true)
        {
            var input = AnsiConsole.Ask<string>("User >");

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (input.Equals("save", StringComparison.OrdinalIgnoreCase))
            {
                await agentSession.SaveAsync();
                AnsiConsole.WriteLine("Session saved.");
                continue;
            }

            agentSession.Ask(input);
            AnsiConsole.MarkupLine("[grey]Thinking...[/]");
            var lineBuilder = new StringBuilder();
            await foreach (var response in agentSession.GetStreamingResponseAsync())
            {
                var split = response.Split('\n');
                for (var i = 0; i < split.Length; i++)
                {
                    var line = split[i];
                    if (i == 0)
                    {
                        lineBuilder.Append(line);
                    }
                    else
                    {
                        AnsiConsole.WriteLine(lineBuilder.ToString());
                        lineBuilder.Clear();
                        lineBuilder.Append(line);
                    }
                }
            }
            AnsiConsole.WriteLine(lineBuilder.ToString());
        }

        AnsiConsole.WriteLine("Goodbye!");
    }
}