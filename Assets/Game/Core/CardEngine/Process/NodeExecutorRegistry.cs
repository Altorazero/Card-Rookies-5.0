using System;
using System.Collections.Generic;

public sealed class NodeExecutorRegistry
{
    private readonly Dictionary<Type, Func<CardNode, CardExecution, NodeOutcome>> _executors = new();

    public void Register<TNode>(INodeExecutor<TNode> executor) where TNode : CardNode
    {
        _executors[typeof(TNode)] = (node, execution) => executor.Execute((TNode)node, execution);
    }

    public Func<CardNode, CardExecution, NodeOutcome> Get(CardNode node)
    {
        if (_executors.TryGetValue(node.GetType(), out var fn))
            return fn;
        throw new InvalidOperationException($"No executor registered for node type: {node.GetType()}");
    }
}