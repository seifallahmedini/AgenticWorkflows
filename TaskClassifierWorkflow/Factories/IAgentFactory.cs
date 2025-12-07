using Microsoft.Agents.AI;

namespace TaskClassifierWorkflow.Factories;

/// <summary>
/// Factory interface for creating AI agents for workflow executors.
/// Follows the Factory Design Pattern to provide abstraction for agent creation.
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Creates an AI agent for a specific executor type.
    /// </summary>
    /// <param name="executorType">The type of executor (e.g., "UpperCase", "Reverse", "Classify")</param>
    /// <param name="instructions">The instructions for the AI agent</param>
    /// <param name="name">Optional name for the agent. If not provided, defaults to executorType</param>
    /// <returns>An AIAgent instance configured for the specified executor type</returns>
    AIAgent CreateAgent(string executorType, string instructions, string? name = null);

    /// <summary>
    /// Creates an AI agent with additional tools/functions.
    /// </summary>
    /// <param name="executorType">The type of executor</param>
    /// <param name="instructions">The instructions for the AI agent</param>
    /// <param name="tools">Array of AI functions/tools to be used by the agent</param>
    /// <param name="name">Optional name for the agent</param>
    /// <returns>An AIAgent instance configured with the specified tools</returns>
    AIAgent CreateAgentWithTools(string executorType, string instructions, IList<Microsoft.Extensions.AI.AITool> tools, string? name = null);
}
