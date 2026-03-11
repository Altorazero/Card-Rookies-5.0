/// <summary>
/// Событие начала хода команды.
/// Генерируется TurnManager при передаче хода конкретной команде.
/// </summary>
public class TurnStartEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Идентификатор команды, чей ход начинается.</summary>
    public Geid TeamId { get; }

    /// <summary>Номер хода (глобальный счётчик).</summary>
    public int TurnNumber { get; }

    public TurnStartEvent(Geid systemSourceId, Geid teamId, int turnNumber)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        TeamId = teamId;
        TurnNumber = turnNumber;
    }
}
