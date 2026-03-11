/// <summary>
/// Событие начала боя.
/// Генерируется при запуске боя; используется для авто-добора карт в начале боя.
/// </summary>
public class BattleStartEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    public BattleStartEvent(Geid systemSourceId)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
    }
}
