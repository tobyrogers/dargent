using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace Dargent.Core;

public class DargentSession(AIAgent aiAgent, AgentSession agentSession)
{
    // private readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
    
    public async Task SaveAsync()
    {
        var jsonString = JsonSerializer.Serialize(agentSession.StateBag, new JsonSerializerOptions(JsonSerializerDefaults.Web){WriteIndented = true});
        Console.WriteLine(jsonString);       
    }

    public IAsyncEnumerable<AgentResponseUpdate> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return aiAgent.RunStreamingAsync(prompt, cancellationToken: cancellationToken, session: agentSession);
    }

    public IAsyncEnumerable<AgentResponseUpdate> ToolApprovalAsync(ToolApprovalResponseContent toolApprovalResponseContent, CancellationToken cancellationToken = default)
    {
        return aiAgent.RunStreamingAsync(new ChatMessage(ChatRole.User, [toolApprovalResponseContent]), session: agentSession, cancellationToken: cancellationToken);
    }
}