/// <summary>
/// Событие конца хода команды.
/// Генерируется TurnManager при завершении хода команды.
/// </summary>
public class TurnEndEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }

    /// <summary>Идентификатор команды, чей ход завершается.</summary>
    public GEID TeamId { get; }

    /// <summary>Номер завершаемого хода.</summary>
    public int TurnNumber { get; }
    public EventScratch Scratch { get; set; }

    public TurnEndEvent(GEID systemSourceId, GEID teamId, int turnNumber)
    {
        Id = GEID.New;
        SystemSourceId = systemSourceId;
        TeamId = teamId;
        TurnNumber = turnNumber;
    }
}
