using System.Collections.Generic;

public class SpendResourceNode : CardNode
{
    public IValueSpec<int> Amount { get; set; }
    public IValueSpec<MetricResourceType> ResourceType { get; set; }
    public IValueSpec<IEntity> Source { get; set; }
    public IValueSpec<IEnumerable<IEntity>> Spenders { get; set; }

    public NodeOutputPort Success { get; }

    public SpendResourceNode()
    {
        Success = AddOutput("Success");
    }
}
public enum MetricResourceType
{
    Mana,
    Health,
    Stamina,
    Energy
}
public class SpendResourceNodeExecutor : INodeExecutor<SpendResourceNode>
{
    public NodeExecutionStatus Execute(SpendResourceNode node, CardExecution execution)
    {
        var spenders = node.Spenders.Resolve(execution.Context);
        var dispatcher = execution.Context.EventContext.Dispatcher;
        foreach (var spender in spenders)
        {
            execution.Context.Bindings.Set(BuiltInBindings.CurrentSpender, spender);
            int amount = node.Amount.Resolve(execution.Context);
            var resourceType = node.ResourceType.Resolve(execution.Context);
            var source = node.Source.Resolve(execution.Context);
            var spendResourceEvent = new SpendResourcesEvent(execution.Context.Card.Id, source, spender, amount, resourceType);
            dispatcher.Enqueue(spendResourceEvent);
        }

        execution.MoveNext(node.Success);
        return NodeExecutionStatus.Completed;
    }
}