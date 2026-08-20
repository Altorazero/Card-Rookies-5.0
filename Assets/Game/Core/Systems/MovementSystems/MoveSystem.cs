using UnityEngine;

public class MoveSystem :
    IEventListener<MoveEvent, IGuardPhaseEvent>,
    IEventListener<MoveEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public GEID SystemId { get; } = GEID.New;

    void IEventListener<MoveEvent, IGuardPhaseEvent>.OnEvent(EventContext context, MoveEvent evt)
    {
        var tgt = evt.GetFirstSubject(Role.Target);
        var entity = tgt;
        if (entity == null)
        {
            Debug.LogWarning($"Entity {tgt} not found.");
            evt.Status = EventStatus.Cancelled;
            return;
        }
        if (entity.GetComponent<HexComponent>() == null)
        {
            Debug.LogWarning($"Entity {entity.Id} has no HexComponent.");
            evt.Status = EventStatus.Cancelled;
        }
    }

    void IEventListener<MoveEvent, IApplyPhaseEvent>.OnEvent(EventContext context, MoveEvent evt)
    {
        var tgt = evt.GetFirstSubject(Role.Target);
        var entity = tgt;
        var positionComp = entity.GetComponent<HexComponent>();
        //positionComp.Coordinates = evt.NewPosition;
        Debug.Log($"DEPRECATED Entity {entity.Id} moved to {evt.NewPosition}");
        evt.Status = EventStatus.Applied;
    }
}
