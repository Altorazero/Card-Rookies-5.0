/// <summary>
/// Система ветвления. Обрабатывает <see cref="BranchingEvent"/> в Apply-фазе:
/// перебирает ветви, ставит в очередь эффекты сработавших ветвей (с учётом лимита).
/// Если ни одна ветвь не сработала и задан DefaultEffect — ставит его в очередь.
/// </summary>
public class BranchingSystem : IEventListener<BranchingEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<BranchingEvent, IApplyPhaseEvent>.OnEvent(EventContext context, BranchingEvent evt)
    {
        int executed = 0;

        foreach (var branch in evt.Branches)
        {
            if (evt.ExecuteLimit > 0 && executed >= evt.ExecuteLimit)
                break;

            bool conditionMet = branch.Condition == null || branch.Condition.Evaluate(context);
            if (!conditionMet)
                continue;

            if (branch.Effect != null)
                context.Dispatcher.Enqueue(branch.Effect);

            executed++;
        }

        if (executed == 0 && evt.DefaultEffect != null)
            context.Dispatcher.Enqueue(evt.DefaultEffect);

        evt.Status = EventStatus.Applied;
    }
}
