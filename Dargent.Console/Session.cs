using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Schema;
using Dargent.Core;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Dargent.Console;

public class Session(AgentSessionFactory agentSessionFactory, IOutput output)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var agentSession = await agentSessionFactory.CreateSession(cancellationToken);

        var figlet = new FigletText("Dargent");
        AnsiConsole.Write(figlet);

        output.Write("Type 'exit' to quit, or 'save' to force save the session.");

        var prompt = ">";
        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await output.Prompt(prompt, cancellationToken);
            output.Write("\n");

            if (string.IsNullOrWhiteSpace(response)) continue;
            if (response.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (response.Equals("save", StringComparison.OrdinalIgnoreCase))
            {
                await agentSession.SaveAsync();
                output.Write("Session saved.");
                continue;
            }

            var (usageDetails, requestContent) = await ProcessResponseContents(agentSession.AskAsync(response, cancellationToken), cancellationToken);
            while (requestContent != null)
            {
                switch (requestContent)
                {
                    case ToolApprovalRequestContent toolApprovalRequestContent:
                        var toolCall = toolApprovalRequestContent.ToolCall;
                        switch (toolCall)
                        {
                            case FunctionCallContent functionCallContent:
                                // request approval
                                var func = $"{functionCallContent.Name}({string.Join(", ", functionCallContent.Arguments?.Select(x=> $"{x.Key}: {x.Value}") ?? [])})";
                                var approved = await output.Approve($"Approve function call {func}\n");
                                (usageDetails, requestContent) = await ProcessResponseContents(
                                    agentSession.ToolApprovalAsync(toolApprovalRequestContent.CreateResponse(approved), cancellationToken), cancellationToken);
                                break;
                            default:
                                output.WriteError($"Unknown tool call type '{nameof(toolCall)}'\n");
                                break;
                        }
                        break;
                    default:
                        output.WriteError($"Unknown request type '{nameof(requestContent)}'\n");
                        break;
                }
            }

            prompt = usageDetails != null
                ? $"\nI:{usageDetails.InputTokenCount} O:{usageDetails.OutputTokenCount} T:{usageDetails.TotalTokenCount} >"
                : ">";
        }

        output.Write("Goodbye!\n");
    }
    
    private async Task<(UsageDetails? usageDetails, InputRequestContent? requestContent)> ProcessResponseContents(IAsyncEnumerable<AgentResponseUpdate> runStreamingAsync, CancellationToken cancellationToken = default)
    {
        InputRequestContent? requestContent = null;
        UsageDetails? usageDetails = null;
        
        await using var enumerator = runStreamingAsync.GetAsyncEnumerator(cancellationToken);

        bool hasFirstValue = false;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Thinking...", async _ =>
            {
                hasFirstValue = await enumerator.MoveNextAsync();
            });
        
        // no first value, just return
        if (!hasFirstValue) return (null, null);

        do
        {
            var response = enumerator.Current;
            if (response.Contents.Count == 0) continue;

            foreach (var content in response.Contents)
                switch (content)
                {
                    case ToolApprovalRequestContent toolApprovalRequestContent:
                        requestContent = toolApprovalRequestContent;
                        break;
                    case FunctionCallContent:
                    case FunctionResultContent:
                        break;
                    case TextContent textContent:
                        output.Write(textContent.Text);
                        break;
                    case UsageContent usageContent:
                        usageDetails = usageContent.Details;
                        break;
                    default:
                        output.WriteError($"Unknown content type '{content.GetType().Name}'\n");
                        break;
                }

        } while (await enumerator.MoveNextAsync());

        return (usageDetails, requestContent);
    }
}