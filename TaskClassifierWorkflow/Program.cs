using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using TaskClassifierWorkflow;
using OpenAI;
using OpenAI.Chat;

// Example: Demonstrating the Agent Factory Design Pattern
// This shows how to create and use AI agents for workflow executors

Console.WriteLine("=== Agent Factory Design Pattern Demo ===\n");

// Load settings from appsettings.json
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var endpoint = config["AzureOpenAI:Endpoint"];
var apiKey = config["AzureOpenAI:Key"];
var deploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4.1";

// Step 1: Create a chat client
AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey!));

IChatClient chatClient = client.GetChatClient(deploymentName)
    .AsIChatClient();

// Step 2: Create the agent factory
IAgentFactory agentFactory = new AgentFactory(chatClient, enableCaching: true);

Console.WriteLine("✓ Agent Factory created\n");

// Step 3: Create agents for different executor types using the factory
var classifierAgent = agentFactory.CreateAgent(
    executorType: "TaskClassifier",
    instructions: "You are a task classifier. Analyze the input text and classify it into categories like 'urgent', 'important', 'routine', or 'informational'. Return ONLY the category name, nothing else.",
    name: "TaskClassifierAgent"
);

var summarizerAgent = agentFactory.CreateAgent(
    executorType: "Summarizer",
    instructions: "You are a text summarizer. Create a concise summary of the input text in one or two sentences.",
    name: "TextSummarizerAgent"
);

var analyzerAgent = agentFactory.CreateAgent(
    executorType: "SentimentAnalyzer",
    instructions: "You are a sentiment analyzer. Analyze the input text and determine if the sentiment is positive, negative, or neutral.",
    name: "SentimentAnalyzerAgent"
);

Console.WriteLine($"✓ Created {((AgentFactory)agentFactory).CachedAgentCount} agents using the factory\n");

// Step 4: Create workflow executors using the agents
var classifierExecutor = new AgentExecutor<string, string>(
    executorId: "ClassifierExecutor",
    agent: classifierAgent,
    inputConverter: input => $"Classify this task: {input}",
    outputConverter: response => ExtractTextFromResponse(response)
);

var summarizerExecutor = new AgentExecutor<string, string>(
    executorId: "SummarizerExecutor",
    agent: summarizerAgent,
    inputConverter: input => $"Summarize this text: {input}",
    outputConverter: response => ExtractTextFromResponse(response)
);

Console.WriteLine("✓ Created workflow executors with agents\n");

// Step 5: Build a workflow using the agent-powered executors
WorkflowBuilder builder = new(classifierExecutor);
builder.AddEdge(classifierExecutor, summarizerExecutor).WithOutputFrom(summarizerExecutor);
var workflow = builder.Build();

Console.WriteLine("✓ Workflow built with agent executors\n");

// Step 6: Execute the workflow with sample input
Console.WriteLine("=== Executing Workflow ===\n");

string inputTask = "Fix the critical security vulnerability in the authentication system that is causing production issues";
Console.WriteLine($"Input Task: {inputTask}\n");

try
{
    await using Run run = await InProcessExecution.RunAsync(workflow, inputTask);
    
    Console.WriteLine("--- Workflow Execution Results ---\n");
    
    foreach (WorkflowEvent evt in run.NewEvents)
    {
        switch (evt)
        {
            case ExecutorInvokedEvent executorInvoked:
                Console.WriteLine($"🔵 [{executorInvoked.ExecutorId}] Started");
                break;

            case ExecutorCompletedEvent executorCompleted:
                Console.WriteLine($"✅ [{executorCompleted.ExecutorId}] Completed");
                Console.WriteLine($"   Result: {executorCompleted.Data}");
                Console.WriteLine();
                break;

            case ExecutorFailedEvent executorFailed:
                Console.WriteLine($"❌ [{executorFailed.ExecutorId}] Failed");
                Console.WriteLine($"   Error: {executorFailed.Data?.Message}");
                Console.WriteLine();
                break;

            case WorkflowOutputEvent workflowOutput:
                Console.WriteLine("🎉 Workflow Completed Successfully!");
                Console.WriteLine($"   Final Output: {workflowOutput.Data}");
                Console.WriteLine();
                break;

            case WorkflowErrorEvent workflowError:
                Console.WriteLine("💥 Workflow Failed!");
                Console.WriteLine($"   Error: {(workflowError.Data as Exception)?.Message}");
                Console.WriteLine();
                break;

            default:
                // Log other events for debugging
                Console.WriteLine($"ℹ️  Event: {evt.GetType().Name}");
                break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error executing workflow: {ex.Message}");
    Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
}

Console.WriteLine("\n=== Demonstrating Agent with Tools ===\n");

var toolAgent = agentFactory.CreateAgentWithTools(
    executorType: "ProcessingAgent",
    instructions: "You are a data processing agent with access to utility functions.",
    tools: new List<AITool>
    {
        AIFunctionFactory.Create(
            (int a, int b) => a + b,
            name: "CalculateSum",
            description: "Calculates the sum of two numbers"
        )
    },
    name: "ProcessingAgent"
);

Console.WriteLine("✓ Agent with tools created\n");

Console.WriteLine("=== Factory Pattern Benefits ===");
Console.WriteLine("✓ Centralized agent creation logic");
Console.WriteLine("✓ Consistent configuration across agents");
Console.WriteLine("✓ Easy to swap implementations");
Console.WriteLine("✓ Support for agent caching");
Console.WriteLine("✓ Extensible for different agent types\n");

Console.WriteLine("Demo completed successfully!");

// Helper method to extract text from agent response
static string ExtractTextFromResponse(AgentRunResponse response)
{
    return string.Join("\n", 
        response.Messages
            .SelectMany(m => m.Contents)
            .OfType<TextContent>()
            .Select(t => t.Text));
}
