/// <summary>
/// Событие конца хода команды.
/// Генерируется TurnManager при завершении хода команды.
/// </summary>
public class TurnEndEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Идентификатор команды, чей ход завершается.</summary>
    public Geid TeamId { get; }

    /// <summary>Номер завершаемого хода.</summary>
    public int TurnNumber { get; }

    public TurnEndEvent(Geid systemSourceId, Geid teamId, int turnNumber)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        TeamId = teamId;
        TurnNumber = turnNumber;
    }
}
