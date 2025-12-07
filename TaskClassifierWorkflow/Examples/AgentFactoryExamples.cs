using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using TaskClassifierWorkflow;

namespace TaskClassifierWorkflow.Examples;

/// <summary>
/// Example demonstrating how to use the Agent Factory pattern with workflows.
/// This example shows various ways to create and use agents for workflow executors.
/// </summary>
public static class AgentFactoryExamples
{
    /// <summary>
    /// Example 1: Creating a simple classification workflow using agent factory.
    /// </summary>
    public static async Task BasicClassificationWorkflowExample(IChatClient chatClient)
    {
        // 1. Create the agent factory
        IAgentFactory factory = new AgentFactory(chatClient, enableCaching: true);

        // 2. Create agents for different tasks
        var classifierAgent = factory.CreateAgent(
            executorType: "TaskClassifier",
            instructions: "Classify tasks as 'urgent', 'important', or 'routine'. Return only the classification.",
            name: "ClassifierAgent"
        );

        var priorityAgent = factory.CreateAgent(
            executorType: "PriorityAssigner",
            instructions: "Assign a priority level (1-5) based on the task classification. Return only the number.",
            name: "PriorityAgent"
        );

        // 3. Create workflow executors
        var classifier = new AgentExecutor<string, string>(
            executorId: "Classifier",
            agent: classifierAgent
        );

        var priorityAssigner = new AgentExecutor<string, string>(
            executorId: "PriorityAssigner",
            agent: priorityAgent
        );

        // 4. Build the workflow
        WorkflowBuilder builder = new(classifier);
        builder.AddEdge(classifier, priorityAssigner).WithOutputFrom(priorityAssigner);
        var workflow = builder.Build();

        // 5. Execute the workflow
        await using Run run = await InProcessExecution.RunAsync(workflow, "Fix critical bug in production");
        
        foreach (WorkflowEvent evt in run.NewEvents)
        {
            if (evt is ExecutorCompletedEvent executorComplete)
            {
                Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
            }
        }
    }

    /// <summary>
    /// Example 2: Creating agents with custom input/output converters.
    /// </summary>
    public static void CustomConvertersExample(IChatClient chatClient)
    {
        IAgentFactory factory = new AgentFactory(chatClient);

        var summarizerAgent = factory.CreateAgent(
            executorType: "Summarizer",
            instructions: "Summarize the input text in exactly one sentence.",
            name: "SummarizerAgent"
        );

        // Custom executor with converters
        var summarizer = new AgentExecutor<TaskData, string>(
            executorId: "TaskSummarizer",
            agent: summarizerAgent,
            inputConverter: task => $"Task: {task.Title}\nDescription: {task.Description}",
            outputConverter: response => 
            {
                var text = string.Join(" ", 
                    response.Messages
                        .SelectMany(m => m.Contents)
                        .OfType<Microsoft.Extensions.AI.TextContent>()
                        .Select(t => t.Text));
                return text.Trim();
            }
        );

        Console.WriteLine("Created task summarizer executor with custom converters");
    }

    /// <summary>
    /// Example 3: Creating agents with tools (function calling).
    /// </summary>
    public static void AgentWithToolsExample(IChatClient chatClient)
    {
        IAgentFactory factory = new AgentFactory(chatClient);

        // Define a tool function
        static string GetTaskDeadline(string taskId)
        {
            // Simulate database lookup
            return taskId switch
            {
                "TASK-001" => "2024-12-31",
                "TASK-002" => "2025-01-15",
                _ => "Not found"
            };
        }

        // Create agent with tools
        var taskManagerAgent = factory.CreateAgentWithTools(
            executorType: "TaskManager",
            instructions: "You help manage tasks. Use the GetTaskDeadline function to retrieve task deadlines.",
            tools: new List<Microsoft.Extensions.AI.AITool>
            {
                Microsoft.Extensions.AI.AIFunctionFactory.Create(GetTaskDeadline)
            },
            name: "TaskManagerAgent"
        );

        Console.WriteLine("Created agent with GetTaskDeadline tool");
    }

    /// <summary>
    /// Example 4: Multi-stage workflow with multiple agents.
    /// </summary>
    public static void MultiStageWorkflowExample(IChatClient chatClient)
    {
        IAgentFactory factory = new AgentFactory(chatClient, enableCaching: true);

        // Create multiple agents for a complex workflow
        var extractorAgent = factory.CreateAgent(
            "DataExtractor",
            "Extract key information from text: who, what, when, where. Return in structured format."
        );

        var validatorAgent = factory.CreateAgent(
            "DataValidator",
            "Validate the extracted information for completeness and accuracy. Return 'VALID' or 'INVALID'."
        );

        var formatterAgent = factory.CreateAgent(
            "DataFormatter",
            "Format the validated data into a professional summary."
        );

        // Create executors
        var extractor = new AgentExecutor<string, string>("Extractor", extractorAgent);
        var validator = new AgentExecutor<string, string>("Validator", validatorAgent);
        var formatter = new AgentExecutor<string, string>("Formatter", formatterAgent);

        // Build workflow: Extract -> Validate -> Format
        WorkflowBuilder builder = new(extractor);
        builder.AddEdge(extractor, validator)
               .AddEdge(validator, formatter)
               .WithOutputFrom(formatter);

        var workflow = builder.Build();

        Console.WriteLine($"Multi-stage workflow created with {((AgentFactory)factory).CachedAgentCount} cached agents");
    }

    /// <summary>
    /// Example 5: Factory pattern benefits - easy to swap implementations.
    /// </summary>
    public static void FactoryBenefitsExample()
    {
        // You can easily create different factory implementations
        // without changing the rest of your code

        // Example: Create a mock factory for testing
        IAgentFactory mockFactory = new MockAgentFactory();

        // Example: Create a factory with different configuration
        // IAgentFactory azureFactory = new AgentFactory(azureClient, enableCaching: true);
        // IAgentFactory openAIFactory = new AgentFactory(openAIClient, enableCaching: false);

        // Your code works with any IAgentFactory implementation
        var agent = mockFactory.CreateAgent(
            "TestExecutor",
            "Test instructions",
            "TestAgent"
        );

        Console.WriteLine("Factory pattern allows easy testing and configuration changes");
    }
}

/// <summary>
/// Sample data model for demonstration.
/// </summary>
public record TaskData(string Title, string Description);

/// <summary>
/// Mock factory implementation for testing.
/// </summary>
public class MockAgentFactory : IAgentFactory
{
    public AIAgent CreateAgent(string executorType, string instructions, string? name = null)
    {
        // Return a mock agent for testing
        Console.WriteLine($"Mock: Creating {executorType} agent");
        throw new NotImplementedException("Use actual IChatClient in production");
    }

    public AIAgent CreateAgentWithTools(string executorType, string instructions, IList<Microsoft.Extensions.AI.AITool> tools, string? name = null)
    {
        Console.WriteLine($"Mock: Creating {executorType} agent with {tools.Count} tools");
        throw new NotImplementedException("Use actual IChatClient in production");
    }
}
