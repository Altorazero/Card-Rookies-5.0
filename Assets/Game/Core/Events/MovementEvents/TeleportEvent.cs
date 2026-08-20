using System.Collections.Generic;

/// <summary>
/// Мгновенное перемещение на целевой гекс.
/// Не вызывает триггеры шагов (как StepMoveEvent), поэтому игнорирует атаки по возможности.
/// </summary>
public class TeleportEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    
    public GEID SystemSourceId { get; }
    public GEID MoverId { get; }
    public HexCoordinates TargetHex { get; }

    public EventScratch Scratch { get; set; }

    public TeleportEvent(GEID systemId, GEID moverId, HexCoordinates targetHex)
    {
        SystemSourceId = systemId;
        MoverId = moverId;
        TargetHex = targetHex;
    }
}
