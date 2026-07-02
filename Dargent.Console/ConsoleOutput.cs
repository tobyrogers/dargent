using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Dargent.Core;

namespace Dargent.Console;

public class ConsoleOutput : IOutput
{
    public void Write(string text, OutputType outputType = OutputType.None)
    {
        try
        {
            if (outputType == OutputType.None)
            {
                AnsiConsole.Write(Regex.Replace(text, @"[{}]", "$0$0"));
                return;
            }

            var colour = outputType switch
            {
                OutputType.Normal => "white",
                OutputType.Error => "red",
                OutputType.Warning => "yellow",
                _ => "white"
            };
            
            
            AnsiConsole.MarkupInterpolated($"[{colour}]{text}[/]");
        }
        catch (Exception e)
        {
            throw new Exception($"Error writing to console: '{text}'", e);
        }
    }
    
    public async Task Spinner(string status, Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spectre.Console.Spinner.Known.Dots)
            .StartAsync(status, _ => action());
    }

    public async Task<bool> Approve(string text, CancellationToken cancellationToken)
    {
        return await AnsiConsole.ConfirmAsync(text, cancellationToken: cancellationToken);
    }

    public async Task<string> Prompt(string prompt, CancellationToken cancellationToken)
    {
        return await AnsiConsole.AskAsync<string>(prompt, cancellationToken);
    }

    public void DrawSeparator(string usage)
    {
        if (string.IsNullOrWhiteSpace(usage))
        {
            AnsiConsole.Write(new Rule());
            return;
        }
        
        AnsiConsole.Write(new Rule($"[blue]{usage}[/]"){ Justification = Justify.Left });
    }
}