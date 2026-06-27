using System.Runtime.CompilerServices;

namespace Dargent.Core;

public class ToolApprovalMiddleware(
    IChatClient innerClient,
    Func<ToolApprovalRequest, Task<ToolApprovalDecision>> approvalCallback)
    : DelegatingChatClient(innerClient)
{
    private readonly HashSet<string> _sessionApprovedTools = new();

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = chatMessages as IList<ChatMessage> ?? chatMessages.ToList();

        await foreach (var update in base.GetStreamingResponseAsync(messageList, options, cancellationToken))
        {
            foreach (var content in update.Contents.OfType<FunctionCallContent>())
            {
                if (_sessionApprovedTools.Contains(content.Name))
                    continue;

                var request = new ToolApprovalRequest
                {
                    ToolName = content.Name,
                    Arguments = (IReadOnlyDictionary<string, object?>?)content.Arguments ?? new Dictionary<string, object?>()
                };

                var decision = await approvalCallback(request);

                if (decision == ToolApprovalDecision.ApproveForSession)
                    _sessionApprovedTools.Add(content.Name);

                if (decision == ToolApprovalDecision.Deny)
                {
                    // Inject a denial result so the model knows the tool was not executed
                    messageList.Add(new ChatMessage(ChatRole.Tool,
                        [new FunctionResultContent(content.CallId, "User denied this tool call.")]));
                    yield break;
                }
            }

            yield return update;
        }
    }
}
