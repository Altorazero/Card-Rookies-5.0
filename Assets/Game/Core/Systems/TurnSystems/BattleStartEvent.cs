/// <summary>
/// Событие начала боя.
/// Генерируется при запуске боя; используется для авто-добора карт в начале боя.
/// </summary>
public class BattleStartEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }

    public EventScratch Scratch { get; set; }

    public BattleStartEvent(GEID systemSourceId)
    {
        Id = GEID.New;
        SystemSourceId = systemSourceId;
    }
}
