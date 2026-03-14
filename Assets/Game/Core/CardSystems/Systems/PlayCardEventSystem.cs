/// <summary>
/// Система разыгрывания карты.
/// Apply-фаза: ставит в очередь все события из <see cref="IPlayingCard.Effects"/>.
/// </summary>
public class PlayCardEventSystem : IEventListener<PlayCardEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<PlayCardEvent, IApplyPhaseEvent>.OnEvent(EventContext context, PlayCardEvent evt)
    {
        foreach (var effect in evt.Card.Effects)
            context.Dispatcher.Enqueue(effect);

        evt.Status = EventStatus.Applied;
    }
}
