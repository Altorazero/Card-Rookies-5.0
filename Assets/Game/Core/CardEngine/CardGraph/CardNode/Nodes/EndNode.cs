public sealed class EndNode : CardNode
{
}

public sealed class EndNodeExecutor
    : INodeExecutor<EndNode>
{

    public NodeExecutionStatus Execute(
        EndNode node,
        CardExecution execution)
    {
        execution.MoveNext((CardNode)null);

        return NodeExecutionStatus.Completed;
    }
}