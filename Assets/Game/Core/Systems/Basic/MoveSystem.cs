using UnityEngine;

public class MoveSystem :
    IEventListener<MoveEntityEvent, IGuardPhaseEvent>,
    IEventListener<MoveEntityEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<MoveEntityEvent, IGuardPhaseEvent>.OnEvent(EventContext context, MoveEntityEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var entity = context.BattleState.GetEntity(tgt);
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

    void IEventListener<MoveEntityEvent, IApplyPhaseEvent>.OnEvent(EventContext context, MoveEntityEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var entity = context.BattleState.GetEntity(tgt);
        var positionComp = entity.GetComponent<HexComponent>();
        positionComp.Coordinates = evt.NewPosition;
        Debug.Log($"Entity {entity.Id} moved to {evt.NewPosition}");
        evt.Status = EventStatus.Applied;
    }
}
