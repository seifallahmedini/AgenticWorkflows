using Microsoft.Agents.AI.Workflows;

namespace SimpleWorkflow;

/// <summary>
/// First executor: converts input text to uppercase.
/// </summary>
internal sealed class UpperCaseExecutor : Executor<string, string>
{
    public UpperCaseExecutor() : base("UpperCaseExecutor")
    {
    }

    public override ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Converts input text to uppercase.
        return ValueTask.FromResult(new string(input.ToUpperInvariant()));
    }
}
