using System.Collections.Generic;

/// <summary>
/// Намерение переместиться. Генерируется игроком (клик на землю) или картой перемещения.
/// Не двигает сущность сразу. Вместо этого перехватывается PathfindingSystem,
/// которая ищет путь (A*) и заменяет это событие на цепочку StepMoveEvent.
/// </summary>
public class PathMoveEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    
    public GEID SystemSourceId { get; }
    public GEID MoverId { get; }
    public HexCoordinates Destination { get; }
    public EventScratch Scratch { get; set; }

    public PathMoveEvent(GEID systemId, GEID moverId, HexCoordinates destination)
    {
        SystemSourceId = systemId;
        MoverId = moverId;
        Destination = destination;
    }
}
