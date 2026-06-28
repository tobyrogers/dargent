using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace Dargent.Core;

public class DargentSession
    (AIAgent aiAgent, AgentSession agentSession, IOutput output)
{
    private readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd-HHmmss");

    public async Task SaveAsync()
    {
        var xx = JsonSerializer.Serialize(agentSession.StateBag, new JsonSerializerOptions(JsonSerializerDefaults.Web){WriteIndented = true});
        
        // JsonElement serializedSession = await aiAgent.SerializeSessionAsync(agentSession, new JsonSerializerOptions(AgentJsonUtilities.DefaultOptions)
        // {
        //     WriteIndented = true, 
        //     //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        // });
        Console.WriteLine(xx);       
    }

    public async IAsyncEnumerable<string> AskAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in aiAgent.RunStreamingAsync(prompt, cancellationToken: cancellationToken, session: agentSession))
        {
            if(response.Contents.Count == 0)
            {
                continue;
            }
            
            foreach (var content in response.Contents)
            {
                switch (content)
                {
                    case ToolApprovalRequestContent toolApprovalRequestContent:
                        var toolCall = toolApprovalRequestContent.ToolCall;
                        switch (toolCall)
                        {
                            case FunctionCallContent functionCallContent:
                                var approved = output.Approve(functionCallContent.Name);
                                await aiAgent.RunAsync(new ChatMessage(ChatRole.User, [toolApprovalRequestContent.CreateResponse(approved)]), session: agentSession, cancellationToken: cancellationToken);
                                break;
                            default:
                                yield return $"Unknown tool call type.{nameof(toolCall)}";
                                break;
                        }
                        break;
                    case FunctionCallContent:
                        break;
                    case TextContent textContent:
                        yield return textContent.Text;
                        break;
                    case UsageContent usageContent:
                        var usage = usageContent.Details;
                        yield return $"\nTokens. In: {usage.InputTokenCount}, Out: {usage.OutputTokenCount}, Total: {usage.TotalTokenCount}\n\n";
                        break;
                    default:
                        yield return $"Unknown content type.{nameof(content)}";
                        break;
                }
            }
            
            // yield return JsonSerializer.Serialize(response) + ",";

            // var xx = response.Contents.OfType<ToolApprovalRequestContent>().ToList();
            // foreach (var toolApprovalRequestContent in xx)
            // {
            //     if (toolApprovalRequestContent.ToolCall is FunctionCallContent fcc)
            //     {
            //         yield return fcc.Name ?? "unk";
            //         await aiAgent.RunAsync(new ChatMessage(ChatRole.User, [toolApprovalRequestContent.CreateResponse(true)]),session: agentSession);
            //     }
            // }
            //
            // yield return $"{response.Role.ToString()} {response.Text} {response.Contents.FirstOrDefault()?.GetType().Name}\n";
            //yield return response.Text;
        }
    }
}