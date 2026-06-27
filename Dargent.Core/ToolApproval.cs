namespace Dargent.Core;

public enum ToolApprovalDecision
{
    ApproveOnce,
    ApproveForSession,
    Deny
}

public class ToolApprovalRequest
{
    public string ToolName { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
}
