using System.Collections.Generic;
using UnityEngine;

public class HealthSystem :
    IEventListener<HealEvent, IModifyPhaseEvent>,
    IEventListener<SingleDamageEvent, IApplyPhaseEvent>,
    ISBAListener
{
    public Geid SystemId { get; } = Geid.New;
    public int Priority { get; } = 100;

    void IEventListener<SingleDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var targetEntity = context.BattleState.GetEntity(tgt);
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                healthComp.CurrentHealth -= evt.DamageAmount;
                Debug.Log($"Entity {targetEntity.Id} took {evt.DamageAmount} damage. Current health: {healthComp.CurrentHealth}");
            }
            else
            {
                Debug.LogWarning($"Entity {targetEntity.Id} has no HealthComponent.");
            }
        }
    }

    void IEventListener<HealEvent, IModifyPhaseEvent>.OnEvent(EventContext context, HealEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var targetEntity = context.BattleState.GetEntity(tgt);
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                healthComp.CurrentHealth = System.Math.Min(healthComp.CurrentHealth + evt.HealAmount, healthComp.MaxHealth);
                Debug.Log($"Entity {targetEntity.Id} healed for {evt.HealAmount}. Current health: {healthComp.CurrentHealth}");
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
        var toRemove = new List<Geid>();

        foreach (var entity in aliveEntities)
        {
            var healthComp = entity.GetComponent<HealthComponent>();
            if (healthComp != null && healthComp.CurrentHealth <= 0)
            {
                Debug.Log($"Entity {entity.Id} has died.");
                toRemove.Add(entity.Id);
            }
        }

        foreach (var id in toRemove)
            context.BattleState.RemoveEntity(id);
    }
}
