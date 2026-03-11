using UnityEngine;

/// <summary>
/// Система стены огня: применяет горение ко всем целям FireWallBurnEvent.
/// Срабатывает в Apply-фазе после того, как TargetingSystem заполнила цели.
/// </summary>
public class FireWallBurnSystem : IEventListener<FireWallBurnEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 110;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<FireWallBurnEvent, IApplyPhaseEvent>.OnEvent(EventContext context, FireWallBurnEvent evt)
    {
        var sourceId = evt.GetFirstSubject(SubjectRole.Source);
        var targets = evt.GetSubjects(SubjectRole.Target);

        foreach (var targetId in targets)
        {
            var applyBurn = new ApplyBurnEvent(evt.SystemSourceId, sourceId, targetId, evt.DamagePerTick, evt.Ticks);
            context.Dispatcher.Enqueue(applyBurn);
        }

        evt.Status = EventStatus.Applied;
        Debug.Log($"[FireWallBurnSystem] Applied burn to {targets.Count} entities.");
    }
}
