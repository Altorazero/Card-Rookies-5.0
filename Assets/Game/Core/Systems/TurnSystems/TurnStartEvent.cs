/// <summary>
/// Событие начала хода команды.
/// Генерируется TurnManager при передаче хода конкретной команде.
/// </summary>
public class TurnStartEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }
    public EventScratch Scratch { get; set; }


    /// <summary>Идентификатор команды, чей ход начинается.</summary>
    public GEID TeamId { get; }

    /// <summary>Номер хода (глобальный счётчик).</summary>
    public int TurnNumber { get; }

    public TurnStartEvent(GEID systemSourceId, GEID teamId, int turnNumber)
    {
        Id = GEID.New;
        SystemSourceId = systemSourceId;
        TeamId = teamId;
        TurnNumber = turnNumber;
    }
}
