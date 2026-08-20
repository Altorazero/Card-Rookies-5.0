public interface INodeExecutor<TNode> where TNode : CardNode
{
    NodeOutcome Execute(TNode node, CardExecution execution);
}