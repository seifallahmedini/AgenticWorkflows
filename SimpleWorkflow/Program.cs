// Build the workflow by connecting executors sequentially
using Microsoft.Agents.AI.Workflows;
using SimpleWorkflow;

var uppercase = new UpperCaseExecutor();
var reverse = new ReverseTextExecutor();

WorkflowBuilder builder = new(uppercase);
builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
var workflow = builder.Build();

// Execute the workflow with input data
await using Run run = await InProcessExecution.RunAsync(workflow, "Hello, World!");
foreach (WorkflowEvent evt in run.NewEvents)
{
    switch (evt)
    {
        case ExecutorCompletedEvent executorComplete:
            Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
            break;
    }
}