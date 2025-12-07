using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace TaskClassifierWorkflow.Factories;

/// <summary>
/// Concrete implementation of IAgentFactory following the Factory Design Pattern.
/// Creates AI agents for workflow executors with configurable chat clients.
/// </summary>
public class AgentFactory : IAgentFactory
{
    private readonly IChatClient _chatClient;
    private readonly Dictionary<string, AIAgent> _agentCache;
    private readonly bool _enableCaching;

    /// <summary>
    /// Initializes a new instance of the AgentFactory.
    /// </summary>
    /// <param name="chatClient">The chat client to use for creating agents (e.g., Azure OpenAI, OpenAI)</param>
    /// <param name="enableCaching">If true, caches created agents for reuse. Default is false.</param>
    public AgentFactory(IChatClient chatClient, bool enableCaching = false)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _enableCaching = enableCaching;
        _agentCache = new Dictionary<string, AIAgent>();
    }

    /// <summary>
    /// Creates an AI agent for a specific executor type.
    /// </summary>
    public AIAgent CreateAgent(string executorType, string instructions, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(executorType))
            throw new ArgumentException("Executor type cannot be null or empty", nameof(executorType));

        if (string.IsNullOrWhiteSpace(instructions))
            throw new ArgumentException("Instructions cannot be null or empty", nameof(instructions));

        var agentName = name ?? $"{executorType}Agent";
        var cacheKey = $"{executorType}_{agentName}";

        // Return cached agent if caching is enabled
        if (_enableCaching && _agentCache.TryGetValue(cacheKey, out var cachedAgent))
        {
            return cachedAgent;
        }

        // Create new agent
        var agent = _chatClient.CreateAIAgent(
            instructions: instructions,
            name: agentName
        );

        // Cache the agent if caching is enabled
        if (_enableCaching)
        {
            _agentCache[cacheKey] = agent;
        }

        return agent;
    }

    /// <summary>
    /// Creates an AI agent with additional tools/functions.
    /// </summary>
    public AIAgent CreateAgentWithTools(string executorType, string instructions, IList<AITool> tools, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(executorType))
            throw new ArgumentException("Executor type cannot be null or empty", nameof(executorType));

        if (string.IsNullOrWhiteSpace(instructions))
            throw new ArgumentException("Instructions cannot be null or empty", nameof(instructions));

        if (tools == null || tools.Count == 0)
            throw new ArgumentException("Tools cannot be null or empty", nameof(tools));

        var agentName = name ?? $"{executorType}Agent";

        // Note: Agents with tools are not cached as they may have different tool configurations
        var agent = _chatClient.CreateAIAgent(
            instructions: instructions,
            name: agentName,
            tools: tools
        );

        return agent;
    }

    /// <summary>
    /// Clears the agent cache. Only applicable when caching is enabled.
    /// </summary>
    public void ClearCache()
    {
        _agentCache.Clear();
    }

    /// <summary>
    /// Gets the number of cached agents.
    /// </summary>
    public int CachedAgentCount => _agentCache.Count;
}
