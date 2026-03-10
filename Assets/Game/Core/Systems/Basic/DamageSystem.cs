using UnityEngine;

public class DamageSystem :
    IEventListener<MassDamageEvent, IGuardPhaseEvent>,
    IEventListener<MassDamageEvent, IModifyPhaseEvent>,
    IEventListener<MassDamageEvent, IApplyPhaseEvent>,
    IEventListener<MassDamageEvent, IAfterPhaseEvent>,
    IEventListener<MassDamageEvent, ITargetResolvePhaseEvent>,
    IEventListener<SingleDamageEvent, IGuardPhaseEvent>,
    IEventListener<SingleDamageEvent, IModifyPhaseEvent>,
    IEventListener<SingleDamageEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 10;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<MassDamageEvent, IGuardPhaseEvent>.OnEvent(EventContext ctx, MassDamageEvent evt)
    {
        if (evt.Amount <= 0)
        {
            evt.Status = EventStatus.Cancelled;
            return;
        }
        if (evt.GetFirstSubject(SubjectRole.Source) == Geid.Empty)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"Damage event {evt.Id} cancelled: no source specified.");
        }
    }

    void IEventListener<MassDamageEvent, ITargetResolvePhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
    }

    void IEventListener<MassDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
        var sourceId = evt.GetFirstSubject(SubjectRole.Source);
        var targets = evt.GetSubjects(SubjectRole.Target);
        foreach (var targetId in targets)
        {
            context.Dispatcher.Enqueue(
                new SingleDamageEvent(evt.SystemSourceId, sourceId, targetId, evt.Amount),
                true);
        }
    }

    void IEventListener<MassDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
    }

    void IEventListener<MassDamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
    }

    void IEventListener<SingleDamageEvent, IGuardPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        if (evt.DamageAmount <= 0)
        {
            evt.Status = EventStatus.Cancelled;
            return;
        }
        if (evt.GetFirstSubject(SubjectRole.Source) == Geid.Empty)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: no source specified.");
            return;
        }
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        if (tgt == Geid.Empty)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: no target specified.");
            return;
        }
        if (context.BattleState.GetEntity(tgt)?.GetComponent<HealthComponent>() == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: target has no HealthComponent.");
        }
    }

    void IEventListener<SingleDamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var srcId = evt.GetFirstSubject(SubjectRole.Source);
        var pwr = context.BattleState.GetEntity(srcId)?.GetComponent<PowerComponent>()?.PowerLevel;
        if (pwr != null)
            evt.DamageAmount += pwr.Value;
    }

    void IEventListener<SingleDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
    }
}
