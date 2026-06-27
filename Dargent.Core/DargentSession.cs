using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

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
            yield return response.Text;
        } 
    }
}