public sealed class StartNode : CardNode
{
    public NodeOutputPort Next { get; }


    public StartNode()
    {
        Next = AddOutput("Next");
    }
}

public sealed class StartNodeExecutor
    : INodeExecutor<StartNode>
{

    public NodeExecutionStatus Execute(
        StartNode node,
        CardExecution execution)
    {
        execution.MoveNext(node.Next);
        return NodeExecutionStatus.Completed;
    }
}