using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
/// <summary>
/// Проверяет и списывает ресурсы (ману и энергию) сущности при обработке события SpendResourcesEvent.
/// </summary>
public class ResourceSystem :
    IEventListener<SpendResourcesEvent, IGuardPhaseEvent>,
    IEventListener<SpendResourcesEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 10;
    public GEID SystemId { get; } = GEID.New;

    void IEventListener<SpendResourcesEvent, IGuardPhaseEvent>.OnEvent(EventContext context, SpendResourcesEvent evt)
    {
        var spender = evt.SpenderEntityId;
        if (spender == null)
        {
            evt.Status = EventStatus.Cancelled;
            return;
        }

        if (evt.ResourceType == MetricResourceType.Mana)
        {
            var mana = spender.GetComponent<ManaComponent>();
            if (mana == null || mana.Current< evt.Amount)
            {
                evt.Status = EventStatus.Cancelled;
                return;
            }
        }

        if (evt.ResourceType == MetricResourceType.Energy)
        {
            var energy = spender.GetComponent<EnergyComponent>();
            if (energy == null || energy.Current < evt.Amount)
            {
                evt.Status = EventStatus.Cancelled;
                return;
            }
        }

        if (evt.ResourceType == MetricResourceType.Health)
        {
            var health = spender.GetComponent<HealthComponent>();
            if (health == null || health.Current < evt.Amount)
            {
                evt.Status = EventStatus.Cancelled;
                return;
            }
        }
    }

    void IEventListener<SpendResourcesEvent, IApplyPhaseEvent>.OnEvent(EventContext context, SpendResourcesEvent evt)
    {
        var spender = evt.SpenderEntityId;
        if (spender == null) return;

        if (evt.ResourceType == MetricResourceType.Mana)
        {
            var mana = spender.GetComponent<ManaComponent>();
            if (mana != null) context.Mutate<ManaComponent>(spender.Id, h =>
            h with { Current = Math.Clamp(h.Current - evt.Amount, 0, h.Max) });
        }

        if (evt.ResourceType == MetricResourceType.Energy)
        {
            var energy = spender.GetComponent<EnergyComponent>();
            if (energy != null) context.Mutate<EnergyComponent>(spender.Id, h =>
            h with { Current = Math.Clamp(h.Current - evt.Amount, 0, h.Max) });
        }
        
        if (evt.ResourceType == MetricResourceType.Health)
        {
            var health = spender.GetComponent<HealthComponent>();
            if (health != null) context.Mutate<HealthComponent>(spender.Id, h =>
            h with { Current = Math.Clamp(h.Current - evt.Amount, 0, h.Max) });
        }
    
        evt.Status = EventStatus.Applied;
    }
}
