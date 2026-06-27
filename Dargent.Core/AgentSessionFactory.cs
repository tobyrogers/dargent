using System.ClientModel;
using Dargent.Core.Tools;
using OpenAI;

namespace Dargent.Core;

public static class AgentSessionFactory
{
    public static AgentSession Create()
    {
        var modelRegistry = new ModelRegistry(GetAgentDirectory());
        var model = modelRegistry.GetDefaultModel();

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(model.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(model.GatewayUrl)
            });

        var client = openAiClient.GetChatClient(model.Name)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        // Configure tools
        var chatOptions = new ChatOptions
        {
            Tools = new List<AITool>
            {
                AIFunctionFactory.Create(AgentTools.ReadFile),
                AIFunctionFactory.Create(AgentTools.WriteFile),
                AIFunctionFactory.Create(AgentTools.RunTerminalCommand)
            }
        };

        IList<ChatMessage> chatHistory = new List<ChatMessage>();
        chatHistory.Add(new ChatMessage(ChatRole.System,
            "You are an AI coding agent named Dargent. You can read/write files and run terminal commands to help the user with coding tasks. Always use the tools when needed."));
        
        //     Console.WriteLine("Loaded previous session history.");
        //     foreach (var msg in history)
        //         if (msg.Role == ChatRole.User || msg.Role == ChatRole.Assistant)
        //         {
        //             if (!string.IsNullOrEmpty(msg.Text)) Console.WriteLine($"{msg.Role}: {msg.Text}");
        //
        //             if (msg.Role == ChatRole.Assistant)
        //                 foreach (var fn in msg.Contents.OfType<FunctionCallContent>())
        //                     Console.WriteLine($"[Tool Call: {fn.Name}]");
        //         }


        return new AgentSession(client, chatOptions, chatHistory);
    }

    private static string GetAgentDirectory()
    {
        string agentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dargent");
        if (!Directory.Exists(agentDirectory))
        {
            Directory.CreateDirectory(agentDirectory);
        }
        return agentDirectory;
    }
}