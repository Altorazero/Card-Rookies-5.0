using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using UnityEngine;

public class DamageSystem : 
    IEventListener<MassDamageEvent, IGuardPhaseEvent>,
    IEventListener<MassDamageEvent, IModifyPhaseEvent>,
    IEventListener<MassDamageEvent, IApplyPhaseEvent>,
    IEventListener<MassDamageEvent, IAfterPhaseEvent>,
    IEventListener<MassDamageEvent, ITargetResolvePhaseEvent>,
    IEventListener<SingleDamageEvent, IGuardPhaseEvent>,
    IEventListener<SingleDamageEvent, IApplyPhaseEvent>,
    IEventListener<SingleDamageEvent, IModifyPhaseEvent>,
    IEventListener<SingleDamageEvent, IAfterPhaseEvent>

{
    public int Priority { get; } = 10;

    public Geid SystemId { get; } = Geid.New;

    void IEventListener<MassDamageEvent, IGuardPhaseEvent>.OnEvent(EventContext ctx, MassDamageEvent evt)
    {
        if (evt.Amount <= 0 || evt.SubjectsList.Count == 0)
        {
            evt.Status = EventStatus.Cancelled;
        }
        var src = evt.SubjectsList.FirstOrDefault(t => t.Role == SubjectRole.Source);
        if (src == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"Damage event {evt.Id} cancelled: no source specified.");
        }

        //var trgt = evt.SubjectsList.FirstOrDefault(t => t.Role == SubjectRole.Target);
        /*        if (trgt == null)
                {
                    evt.Status = EventStatus.Cancelled;
                    Debug.LogWarning($"Damage event {evt.Id} cancelled: no target specified.");
                }*/
    }

    void IEventListener<MassDamageEvent, ITargetResolvePhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
       
    }

    void IEventListener<MassDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
        var targets = evt.SubjectsList.Where(t => t.Role == SubjectRole.Target);
        foreach (var target in targets)
        {
            context.Dispatcher.Enqueue(new SingleDamageEvent(evt.SystemSourceId, 
                evt.SubjectsList.FirstOrDefault(t => t.Role == SubjectRole.Source).Entity, 
                target.Entity, evt.Amount), true);
        }
    }
    void IEventListener<MassDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {

    }

    void IEventListener<SingleDamageEvent, IGuardPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        if (evt.DamageAmount <= 0)
        {
            evt.Status = EventStatus.Cancelled;
        }
        if (evt.SubjectsList.Count == 0)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: no subjects specified.");
            return;
        }
        if (!evt.SubjectsList.Any(t => t.Role == SubjectRole.Source))
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: no source specified.");
            return;

        }
        if (evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target) == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: target must be exactly one.");
            return;

        }
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;

        if (context.BattleState.GetEntity(tgt)?.GetComponent<HealthComponent>() == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"SingleDamageEvent {evt.Id} cancelled: target has no HealthComponent.");
            return;

        }
    }
    void IEventListener<SingleDamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var src = evt.SubjectsList.FirstOrDefault(t => t.Role == SubjectRole.Source);
        var pwr = context.BattleState.GetEntity(src.Entity).GetComponent<PowerComponent>()?.PowerLevel;
        if (pwr != null)
        {
            evt.DamageAmount += pwr.Value;
        }
        
    }
    void IEventListener<SingleDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        context.BattleState.GetEntity(tgt).GetComponent<HealthComponent>().CurrentHealth -= evt.DamageAmount;
    }
    void IEventListener<SingleDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        
    }

    void IEventListener<MassDamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, MassDamageEvent evt)
    {
        
    }
}
