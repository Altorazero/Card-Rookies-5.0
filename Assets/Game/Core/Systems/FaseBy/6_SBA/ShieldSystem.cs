using System;
using UnityEngine;

public class ShieldSystem : IEventListener<DamageEvent, IModifyPhaseEvent>
{
    public int Priority { get; } = 20;
    public GEID SystemId { get; } = GEID.New;

    void IEventListener<DamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, DamageEvent evt)
    {
        var tgt = evt.GetFirstSubject(Role.Target);
        var shieldComp = tgt?.GetComponent<ShieldComponent>();

        if (shieldComp != null && shieldComp.Value > 0)
        {
            int damageToShield = Math.Min(evt.Amount, shieldComp.Value);
            //shieldComp.Value -= damageToShield;
            evt.Amount -= damageToShield;
            Debug.Log($" DEPRECATED shield Entity {tgt?.Id} absorbed {damageToShield} damage with shield. Remaining shield: {shieldComp.Value}");
            if (evt.Amount <= 0)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"DEPRECATED shield All damage absorbed by shield for entity {tgt}. Damage event cancelled.");
            }
        }
    }
}
