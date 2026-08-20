using System.Collections.Generic;

public class DamageNode : CardNode
{
    public IValueSpec<int> Amount { get; set; }
    public IValueSpec<DamageType> DamageType { get; set; }
    public IValueSpec<IEntity> Source { get; set; }
    public IValueSpec<IEnumerable<IEntity>> Targets { get; set; }

    public NodeOutputPort Success { get; }

    public DamageNode()
    {
        Success = AddOutput("Success");
    }
}

public sealed class DamageNodeExecutor : INodeExecutor<DamageNode>
{
    public NodeOutcome Execute(DamageNode node, CardExecution execution)
    {
        var context = execution.Context;
        var targets = node.Targets.Resolve(context);
        var amount = node.Amount.Resolve(context);
        var damageType = node.DamageType.Resolve(context);
        var source = node.Source.Resolve(context);

        foreach (var target in targets)
            execution.Emit(new DamageEvent(context.Card.Id, source, target, amount, damageType));

        return NodeOutcome.Advance(execution.Graph.GetNext(node.Success));
    }
}