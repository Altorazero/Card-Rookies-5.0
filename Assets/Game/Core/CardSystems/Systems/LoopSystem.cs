/// <summary>
/// Система цикла. Обрабатывает <see cref="LoopEvent"/> в Apply-фазе:
/// проверяет условие продолжения через <see cref="ILoopState.ShouldContinue"/>,
/// ставит в очередь эффект шага и следующий <see cref="LoopEvent"/> с новым состоянием.
/// </summary>
public class LoopSystem : IEventListener<LoopEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<LoopEvent, IApplyPhaseEvent>.OnEvent(EventContext context, LoopEvent evt)
    {
        if (!evt.State.ShouldContinue(context))
        {
            evt.Status = EventStatus.Applied;
            return;
        }

        var step = evt.State.CreateStepEffect(context);
        if (step != null)
            context.Dispatcher.Enqueue(step);

        var nextState = evt.State.Advance(context);
        if (nextState != null)
        {
            var nextLoop = new LoopEvent(evt.SystemSourceId, nextState);
            context.Dispatcher.Enqueue(nextLoop);
        }

        evt.Status = EventStatus.Applied;
    }
}
