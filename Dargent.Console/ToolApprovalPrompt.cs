using Dargent.Core;
using System.Text.Json;

namespace Dargent.Console;

public static class ToolApprovalPrompt
{
    public static Task<ToolApprovalDecision> RequestApprovalAsync(ToolApprovalRequest request)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[yellow]Agent wants to use tool:[/] [bold]{request.ToolName}[/]");

        if (request.Arguments.Count > 0)
        {
            var argsJson = JsonSerializer.Serialize(request.Arguments, new JsonSerializerOptions { WriteIndented = true });
            AnsiConsole.MarkupLine("[grey]Arguments:[/]");
            AnsiConsole.WriteLine(argsJson);
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Allow this tool call?")
                .AddChoices("Approve once", "Approve for session", "Deny"));

        var decision = choice switch
        {
            "Approve once" => ToolApprovalDecision.ApproveOnce,
            "Approve for session" => ToolApprovalDecision.ApproveForSession,
            _ => ToolApprovalDecision.Deny
        };

        return Task.FromResult(decision);
    }
}
