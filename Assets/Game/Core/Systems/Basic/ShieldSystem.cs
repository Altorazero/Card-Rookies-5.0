using System;
using UnityEngine;

public class ShieldSystem : IEventListener<SingleDamageEvent, IModifyPhaseEvent>
{
    public int Priority { get; } = 20;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<SingleDamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var shieldComp = context.BattleState.GetEntity(tgt)?.GetComponent<ShieldComponent>();

        if (shieldComp != null && shieldComp.ShieldValue > 0)
        {
            int damageToShield = Math.Min(evt.DamageAmount, shieldComp.ShieldValue);
            shieldComp.ShieldValue -= damageToShield;
            evt.DamageAmount -= damageToShield;
            Debug.Log($"Entity {tgt} absorbed {damageToShield} damage with shield. Remaining shield: {shieldComp.ShieldValue}");
            if (evt.DamageAmount <= 0)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"All damage absorbed by shield for entity {tgt}. Damage event cancelled.");
            }
        }
    }
}
