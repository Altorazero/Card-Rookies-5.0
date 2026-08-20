using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResolveBindingNode<T> : CardNode
{
    public BindingKey<IEnumerable<T>> Output;

    [SerializeReference]
    public ICandidateProvider<T> Provider;

    [SerializeReference]
    public List<ICandidateTransform<T>> Transforms = new();

    [SerializeReference]
    public ISelector<T> Selector;

    public NodeOutputPort Success;

    public ResolveBindingNode()
    {
        Success = AddOutput("Success");
    }
}

public sealed class ResolveBindingNodeExecutor<T> : INodeExecutor<ResolveBindingNode<T>>
{
    public NodeOutcome Execute(ResolveBindingNode<T> node, CardExecution execution)
    {
        var context = execution.Context;
        var bindings = context.Bindings;

        // Preview/hover: если для этого узла уже есть предзаданный результат —
        // не спрашиваем IInteractionService вообще, узел не приостанавливается.
        if (context.SelectionOverrides != null &&
            context.SelectionOverrides.TryGetOverride(node, node.Output, out var predetermined))
        {
            bindings.Set(node.Output, predetermined);
            return NodeOutcome.Advance(execution.Graph.GetNext(node.Success));
        }

        IEnumerable<T> candidates = node.Provider.GetValues(context);
        foreach (var transform in node.Transforms)
            candidates = transform.Transform(candidates, context);

        SelectionResult<T> result = node.Selector.Select(candidates, context);

        if (result.IsCompleted)
        {
            bindings.Set(node.Output, result.Value);
            return NodeOutcome.Advance(execution.Graph.GetNext(node.Success));
        }

        var interactionService = context.EventContext.Interaction;
        if (interactionService == null)
        {
            UnityEngine.Debug.LogError("No IInteractionService provided, but a pending selection was reached.");
            bindings.Set(node.Output, Array.Empty<T>());
            return NodeOutcome.Advance(execution.Graph.GetNext(node.Success));
        }

        var suspendPoint = new SelectionSuspendPoint<T>(node.Id, result.Execution);
        suspendPoint.Resolved += () => bindings.Set(node.Output, suspendPoint.Result);

        interactionService.RequestTargetSelection(result.Execution);
        return NodeOutcome.Suspend(suspendPoint);
    }
}
public interface ISelectionOverrideSource
{
    bool TryGetOverride<T>(CardNode node, BindingKey<IEnumerable<T>> outputKey, out IEnumerable<T> value);
}