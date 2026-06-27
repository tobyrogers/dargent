using System.Text;
using System.Threading;
using Dargent.Core;

namespace Dargent.Console;

public class Session(AgentSessionFactory agentSessionFactory)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var agentSession = agentSessionFactory.CreateSession();

        AnsiConsole.WriteLine("Argent session started\nType 'exit' to quit, or 'save' to force save the session.");

        while (cancellationToken.IsCancellationRequested == false)
        {
            var input = await AnsiConsole.AskAsync<string>("User >", cancellationToken);

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (input.Equals("save", StringComparison.OrdinalIgnoreCase))
            {
                await agentSession.SaveAsync();
                AnsiConsole.WriteLine("Session saved.");
                continue;
            }

            agentSession.Ask(input);
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .AutoRefresh(true)
                .StartAsync("Thinking...", async ctx =>
                {
                    var lineBuilder = new StringBuilder();
                    await foreach (var response in agentSession.GetStreamingResponseAsync(cancellationToken))
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
                });
        }

        AnsiConsole.WriteLine("Goodbye!");
    }
}