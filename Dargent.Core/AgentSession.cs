using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dargent.Core;

public class AgentSession : IDisposable
{
    private readonly IChatClient _chatClient;
    private readonly IList<ChatMessage> _chatHistory;
    private readonly ChatOptions? _options;
    private readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd-HHmmss");

    public AgentSession(IChatClient chatClient, ChatOptions options, IList<ChatMessage> chatHistory)
    {
        _chatClient = chatClient;
        _options = options;
        _chatHistory = chatHistory;
    }

    public void Dispose()
    {
        _chatClient.Dispose();
    }

    public void Ask(string question)
    {
        _chatHistory.Add(new ChatMessage(ChatRole.User, question));
    }

    public async Task SaveAsync()
    {
        await SessionManager.SaveSession(_sessionId, _chatHistory);
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var responseAggregator = new StringBuilder();
        await foreach (var response in _chatClient.GetStreamingResponseAsync(_chatHistory, _options, cancellationToken))
        {
            responseAggregator.Append(response.Text);
            yield return response.Text;
        }

        _chatHistory.Add(new ChatMessage(ChatRole.Assistant, responseAggregator.ToString()));
        await SaveAsync();
    }
}