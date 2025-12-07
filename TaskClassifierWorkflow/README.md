# TaskClassifierWorkflow

A .NET 10 project demonstrating the **Factory Design Pattern** for creating AI agents that power workflow executors using Microsoft Agent Framework and Azure OpenAI.

## ?? Project Structure

```
TaskClassifierWorkflow/
??? ?? Factories/               # Agent factory implementations
?   ??? IAgentFactory.cs       # Factory interface
?   ??? AgentFactory.cs        # Concrete factory implementation
?
??? ?? Executors/              # Workflow executor implementations
?   ??? AgentExecutor.cs       # Generic AI agent executor wrapper
?
??? ?? Models/                 # Data models and DTOs
?   ??? TaskData.cs            # Task data model
?
??? ?? Examples/               # Usage examples and demonstrations
?   ??? AgentFactoryExamples.cs # Factory pattern examples
?
??? ?? Documentation/          # Project documentation
?   ??? AGENT_FACTORY_README.md        # Factory pattern documentation
?   ??? WORKFLOW_ACTIVITY_DIAGRAM.md   # Activity diagrams
?
??? Program.cs                 # Application entry point
??? appsettings.json          # Configuration file
??? TaskClassifierWorkflow.csproj # Project file
```

## ?? Key Features

### **Factory Design Pattern**
- Centralized agent creation with `IAgentFactory` interface
- Support for agent caching to improve performance
- Easy to test with mock implementations

### **Agent Executors**
- Generic `AgentExecutor<TInput, TOutput>` for flexible workflows
- Custom input/output converters
- Seamless integration with Microsoft Agent Framework workflows

### **Workflow Support**
- Sequential workflow execution
- Event-driven architecture with comprehensive logging
- Error handling and recovery mechanisms

## ?? Getting Started

### Prerequisites
- .NET 10 SDK
- Azure OpenAI API access
- Visual Studio 2022 or later (or VS Code with C# extension)

### Configuration

Update `appsettings.json` with your Azure OpenAI credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "Key": "your-api-key",
    "DeploymentName": "gpt-4"
  }
}
```

### Running the Application

```bash
dotnet run --project TaskClassifierWorkflow
```

## ?? Usage Examples

### Basic Factory Usage

```csharp
using TaskClassifierWorkflow.Factories;
using TaskClassifierWorkflow.Executors;

// Create factory
IAgentFactory factory = new AgentFactory(chatClient, enableCaching: true);

// Create an agent
var agent = factory.CreateAgent(
    executorType: "TaskClassifier",
    instructions: "Classify tasks into categories",
    name: "ClassifierAgent"
);

// Create executor
var executor = new AgentExecutor<string, string>(
    executorId: "Classifier",
    agent: agent
);
```

### Building Workflows

```csharp
// Build workflow
WorkflowBuilder builder = new(classifierExecutor);
builder.AddEdge(classifierExecutor, summarizerExecutor)
       .WithOutputFrom(summarizerExecutor);
var workflow = builder.Build();

// Execute
await using Run run = await InProcessExecution.RunAsync(workflow, inputTask);
```

See `Examples/AgentFactoryExamples.cs` for more comprehensive examples.

## ??? Architecture

### Design Patterns
- **Factory Pattern**: Centralized agent creation
- **Strategy Pattern**: Input/output converters
- **Template Method Pattern**: Base executor class

### Key Components

#### 1. **Factories**
Responsible for creating and managing AI agents with consistent configuration.

#### 2. **Executors**
Wrapper classes that integrate AI agents into workflow execution pipelines.

#### 3. **Models**
Data transfer objects and domain models used throughout the application.

## ?? Documentation

- **[Agent Factory Documentation](Documentation/AGENT_FACTORY_README.md)** - Detailed factory pattern guide
- **[Workflow Activity Diagrams](Documentation/WORKFLOW_ACTIVITY_DIAGRAM.md)** - Visual workflow representations

## ?? Testing

The project includes mock factory implementations for unit testing:

```csharp
IAgentFactory mockFactory = new MockAgentFactory();
```

## ?? Technologies

- **.NET 10** - Latest .NET framework
- **Microsoft.Agents.AI** - AI agent framework
- **Microsoft.Agents.AI.Workflows** - Workflow orchestration
- **Azure.AI.OpenAI** - Azure OpenAI integration
- **Microsoft.Extensions.AI** - AI abstractions

## ?? NuGet Packages

```xml
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
<PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.251204.1" />
<PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.251204.1" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.251110.2" />
<PackageReference Include="Azure.Identity" Version="1.17.0" />
```

## ?? Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns and conventions
- Namespaces match folder structure
- Documentation is updated for new features

## ?? License

This project is part of the SimpleWorkflow solution.

## ?? Related Projects

- [SimpleWorkflow](../SimpleWorkflow/) - Basic workflow implementation

---

*Generated for .NET 10 - December 2024*
