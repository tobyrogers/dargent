using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.FileProviders;

namespace Dargent.Core;

public class DargentSession(AIAgent aiAgent, AgentSession agentSession)
{
    private readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd-HHmmss");

    public async Task SaveAsync()
    {
        JsonElement serializedSession = await aiAgent.SerializeSessionAsync(agentSession);
        Console.WriteLine(serializedSession.ToString());       
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
                yield return $"{content.GetType().Name}: ";
                switch (content)
                {
                    case ToolApprovalRequestContent toolApprovalRequestContent:
                        var toolCall = toolApprovalRequestContent.ToolCall;
                        switch (toolCall)
                        {
                            case FunctionCallContent functionCallContent:
                                yield return $"{functionCallContent.Name}";
                                break;
                                // await aiAgent.RunAsync(new ChatMessage(ChatRole.User,
                                // [toolApprovalRequestContent.CreateResponse(true)]), session: agentSession);
                        }
                        break;
                    case FunctionCallContent:
                        break;
                    case TextContent textContent:
                        yield return textContent.Text;
                        break;
                    case UsageContent usageContent:
                        var usage = usageContent.Details;
                        yield return $"Tokens. In: {usage.InputTokenCount}, Out: {usage.OutputTokenCount}, Total: {usage.TotalTokenCount} ";
                        break;
                }
            }
            yield return "\n"; 
            
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