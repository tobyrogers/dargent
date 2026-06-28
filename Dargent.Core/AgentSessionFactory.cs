using System.ClientModel;
using System.Threading.Tasks;
using Dargent.Core.Tools;
using Microsoft.Agents.AI;
using OpenAI;

namespace Dargent.Core;

public class AgentSessionFactory()
{
    public async Task<DargentSession> CreateSession(CancellationToken cancellationToken = default)
    {
        var modelRegistry = new ModelRegistry(GetAgentDirectory());
        var model = modelRegistry.GetDefaultModel();

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(model.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(model.GatewayUrl)
            });

        var chatClient = openAiClient.GetChatClient(model.Name)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        var chatClientAgentOptions = new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Tools = new List<AITool>
                {
                    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(AgentTools.ReadFile)),
                    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(AgentTools.WriteFile)),
                    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(AgentTools.RunTerminalCommand))
                }
            }
        };
        
        AIAgent aiAgent = chatClient.AsAIAgent(chatClientAgentOptions);
        
        //     "You are an AI coding agent named Dargent. You can read/write files and run terminal commands to help the user with coding tasks. Always use the tools when needed."));
        var agentSession = await aiAgent.CreateSessionAsync(cancellationToken);
        return new DargentSession(aiAgent, agentSession);
    }

    private static string GetAgentDirectory()
    {
        var agentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dargent");
        if (!Directory.Exists(agentDirectory)) Directory.CreateDirectory(agentDirectory);
        return agentDirectory;
    }
}