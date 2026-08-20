using System.Collections.Generic;

/// <summary>
/// Атомарный шаг на один соседний гекс.
/// </summary>
public class StepMoveEvent : IGameEvent, IGuardPhaseEvent, IApplyPhaseEvent, IAfterPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    
    public GEID SystemSourceId { get; }
    public GEID MoverId { get; }
    public HexCoordinates TargetHex { get; }
    public HexCoordinates PreviousHex { get; set; } // Заполняется на фазе Apply для триггеров
    public EventScratch Scratch { get; set; }

    public StepMoveEvent(GEID systemId, GEID moverId, HexCoordinates targetHex)
    {
        SystemSourceId = systemId;
        MoverId = moverId;
        TargetHex = targetHex;
    }
}
