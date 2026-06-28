using System.Text;
using System.Threading;
using Dargent.Core;

namespace Dargent.Console;

public class Session(AgentSessionFactory agentSessionFactory, IOutput output)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var agentSession = await agentSessionFactory.CreateSession(cancellationToken);

        output.Write("Argent session started\nType 'exit' to quit, or 'save' to force save the session.");

        while (cancellationToken.IsCancellationRequested == false)
        {
            var prompt = await output.Prompt(cancellationToken);

            if (string.IsNullOrWhiteSpace(prompt)) continue;
            if (prompt.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (prompt.Equals("save", StringComparison.OrdinalIgnoreCase))
            {
                await agentSession.SaveAsync();
                output.Write("Session saved.");
                continue;
            }
            
            await output.Stream(agentSession.AskAsync(prompt, cancellationToken));

            // await AnsiConsole.Status()
            //     .Spinner(Spinner.Known.Dots)
            //     .AutoRefresh(true)
            //     .StartAsync("Thinking...", async ctx =>
            //     {
            //         var lineBuilder = new StringBuilder();
            //         await foreach (var response in agentSession.AskAsync(prompt, cancellationToken))
            //         {
            //             var split = response.Split('\n');
            //             for (var i = 0; i < split.Length; i++)
            //             {
            //                 var line = split[i];
            //                 if (i == 0)
            //                 {
            //                     lineBuilder.Append(line);
            //                 }
            //                 else
            //                 {
            //                     AnsiConsole.WriteLine(lineBuilder.ToString());
            //                     lineBuilder.Clear();
            //                     lineBuilder.Append(line);
            //                 }
            //             }
            //         }
            //         AnsiConsole.WriteLine(lineBuilder.ToString());
            //     });
        }

        AnsiConsole.WriteLine("Goodbye!");
    }
}