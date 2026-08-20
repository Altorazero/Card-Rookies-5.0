using UnityEngine;

public class PlayCardEventSystem : IEventListener<PlayCardEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public GEID SystemId { get; } = GEID.New;

    private static readonly NodeExecutorRegistry Registry = BuildRegistry();

    private static NodeExecutorRegistry BuildRegistry()
    {
        var registry = new NodeExecutorRegistry();
        registry.Register<StartNode>(new StartNodeExecutor());
        registry.Register<EndNode>(new EndNodeExecutor());
        registry.Register<ResolveBindingNode<IEntity>>(new ResolveBindingNodeExecutor<IEntity>());
        registry.Register<DamageNode>(new DamageNodeExecutor());
        registry.Register<SpendResourceNode>(new SpendResourceNodeExecutor());
        return registry;
    }

    public void OnEvent(EventContext context, PlayCardEvent evt)
    {
        var card = evt.Card;
        if (card == null || card.Definition == null || card.Definition.CardGraph == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning("[PlayCardEventSystem] Play cancelled: Card has no valid graph.");
            return;
        }

        var execContext = new ExecutionContext(context, card);
        execContext.Bindings.Set(BuiltInBindings.Caster, evt.Caster);
        execContext.Bindings.Set(BuiltInBindings.Spenders, new[] { evt.Caster });

        var dispatcher = context.Dispatcher; // захватываем для отложенного Enqueue
        var host = new EventQueueCardExecutionHost(dispatcher);
        var execution = new CardExecution(card.Definition.CardGraph, execContext, Registry, host);
        evt.Execution = execution;

        execution.Completed += _ =>
        {
            // Настоящие последствия розыгрыша (урон, лечение...) не были применены
            // напрямую — они собраны графом как события и кладутся в очередь ТОЛЬКО
            // сейчас, после PlayCardEvent, чтобы каждое честно прошло все 7 фаз само.
            foreach (var produced in execution.ProducedEvents)
                dispatcher.Enqueue(produced);

            evt.Status = EventStatus.Applied;
            Debug.Log($"[PlayCardEventSystem] Card {card.Definition.CardName} played successfully.");
        };
        execution.Suspended += _ =>
        {
            evt.Status = EventStatus.WaitingForInput;
            Debug.Log($"[PlayCardEventSystem] Card {card.Definition.CardName} paused, waiting for target selection.");
        };
        execution.Cancelled += _ =>
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"[PlayCardEventSystem] Card {card.Definition.CardName} execution cancelled mid-graph.");
        };

        execution.Tick();
    }
}