using System.Collections.Generic;
using UnityEngine;

public class HealthSystem :
    IEventListener<HealEvent, IModifyPhaseEvent>,
    IEventListener<DamageEvent, IApplyPhaseEvent>,
    ISBAListener
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority { get; } = 100;

    void IEventListener<DamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, DamageEvent evt)
    {
        var tgt = evt.GetFirstSubject(Role.Target);
        var targetEntity = tgt;
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                context.BattleState.Mutate<HealthComponent>(context.CommandLog, targetEntity.Id, hc => hc with { Current = System.Math.Min(hc.Current - evt.Amount, hc.Max) });
                Debug.Log($"Entity {targetEntity.Id} took {evt.Amount} damage. Current health: {healthComp.Current}");
            }
            else
            {
                Debug.LogWarning($"Entity {targetEntity.Id} has no HealthComponent.");
            }
        }
    }

    void IEventListener<HealEvent, IModifyPhaseEvent>.OnEvent(EventContext context, HealEvent evt)
    {
        var tgt = evt.GetFirstSubject(Role.Target);
        var targetEntity = tgt;
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                context.BattleState.Mutate<HealthComponent>(context.CommandLog, targetEntity.Id, hc => hc with { Current = System.Math.Min(hc.Current + evt.Amount, hc.Max) });
                Debug.Log($"Entity {targetEntity.Id} healed for {evt.Amount}. Current health: {healthComp.Current}");
            }
            else
            {
                Debug.LogWarning($"Entity {targetEntity.Id} has no HealthComponent.");
            }
        }
    }

    void ISBAListener.OnSBA(EventContext context)
    {
        var aliveEntities = context.BattleState.Entities.Values;
        var toRemove = new List<GEID>();

        foreach (var entity in aliveEntities)
        {
            var healthComp = entity.GetComponent<HealthComponent>();
            if (healthComp != null && healthComp.Current <= 0)
            {
                Debug.Log($"Entity {entity.Id} has died.");
                toRemove.Add(entity.Id);
            }
        }

        foreach (var id in toRemove)
            context.BattleState.RemoveEntity(id);
    }
}
