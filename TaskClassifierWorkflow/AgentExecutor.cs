using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace TaskClassifierWorkflow;

/// <summary>
/// Executor wrapper that integrates AI agents into workflow executors.
/// This class demonstrates how to use agents created by the factory within a workflow.
/// </summary>
/// <typeparam name="TInput">The input type for the executor</typeparam>
/// <typeparam name="TOutput">The output type for the executor</typeparam>
public class AgentExecutor<TInput, TOutput> : Executor<TInput, TOutput>
{
    private readonly AIAgent _agent;
    private readonly Func<TInput, string>? _inputConverter;
    private readonly Func<AgentRunResponse, TOutput>? _outputConverter;

    /// <summary>
    /// Initializes a new instance of AgentExecutor.
    /// </summary>
    /// <param name="executorId">Unique identifier for the executor</param>
    /// <param name="agent">The AI agent created by the factory</param>
    /// <param name="inputConverter">Optional converter to transform input to a prompt string</param>
    /// <param name="outputConverter">Optional converter to transform agent response to output type</param>
    public AgentExecutor(
        string executorId,
        AIAgent agent,
        Func<TInput, string>? inputConverter = null,
        Func<AgentRunResponse, TOutput>? outputConverter = null)
        : base(executorId)
    {
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _inputConverter = inputConverter;
        _outputConverter = outputConverter;
    }

    /// <summary>
    /// Handles the execution using the AI agent.
    /// </summary>
    public override async ValueTask<TOutput> HandleAsync(TInput input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert input to prompt if converter is provided
            string prompt = _inputConverter != null
                ? _inputConverter(input)
                : input?.ToString() ?? string.Empty;

            // Run the agent with the prompt
            AgentRunResponse agentResponse = await _agent.RunAsync(prompt, cancellationToken: cancellationToken);

            // Convert agent response to output type
            if (_outputConverter != null)
            {
                return _outputConverter(agentResponse);
            }

            // If no output converter and output type is string, extract text from response
            if (typeof(TOutput) == typeof(string))
            {
                // Extract text content from the agent response messages
                var textContent = string.Join("\n", 
                    agentResponse.Messages
                        .SelectMany(m => m.Contents)
                        .OfType<Microsoft.Extensions.AI.TextContent>()
                        .Select(t => t.Text));
                
                return (TOutput)(object)textContent;
            }

            // If output type is AgentRunResponse, return directly
            if (typeof(TOutput) == typeof(AgentRunResponse))
            {
                return (TOutput)(object)agentResponse;
            }

            throw new InvalidOperationException(
                $"Cannot convert agent response to {typeof(TOutput).Name}. Provide an output converter.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error executing agent in executor '{Id}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the underlying AI agent.
    /// </summary>
    public AIAgent Agent => _agent;
}
