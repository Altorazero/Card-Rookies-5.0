using System;
using UnityEngine;

public sealed class DamageApplySystem : IEventListener<DamageEvent, IApplyPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 10; // позже, чем списание щита

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        if (evt.Amount <= 0) return;

        var target = evt.GetFirstSubject(Role.Target);
        context.Mutate<HealthComponent>(target.Id, h =>
            h with { Current = Math.Clamp(h.Current - evt.Amount, 0, h.Max) });

        if (context.IsReal)
            Debug.Log($"Entity {target.Id} took {evt.Amount} damage.");
    }
}