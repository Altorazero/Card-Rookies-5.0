using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class MoveSystem : 
    IEventListener<MoveEntityEvent, IGuardPhaseEvent>,
    IEventListener<MoveEntityEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;

    public Geid SystemId { get; } = Geid.New;
    void IEventListener<MoveEntityEvent, IGuardPhaseEvent>.OnEvent(EventContext context, MoveEntityEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        var entity = context.BattleState.GetEntity(tgt);
        var positionComp = entity?.GetComponent<HexComponent>();
        if (entity == null)
        {
            Debug.LogWarning($"Entity {tgt} not found.");
            evt.Status = EventStatus.Cancelled;
            return;
        }

        if (positionComp == null)
        {
            Debug.LogWarning($"Entity {entity.Id} has no PositionComponent.");
            evt.Status = EventStatus.Cancelled;
            return;
        }
    }

    void IEventListener<MoveEntityEvent, IApplyPhaseEvent>.OnEvent(EventContext context, MoveEntityEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        var entity = context.BattleState.GetEntity(tgt);

        var positionComp = entity.GetComponent<HexComponent>();
        positionComp.Coordinates = evt.NewPosition;
        Debug.Log($"Entity {entity.Id} moved to {evt.NewPosition}");
        evt.Status = EventStatus.Applied;
    }
}