using UnityEngine;

/// <summary>
/// Система трат ресурсов (мана, энергия).
/// Guard-фаза: проверяет, достаточно ли у сущности ресурсов. При нехватке — отменяет событие.
/// Apply-фаза: списывает ресурсы.
/// </summary>
public class ResourceCostSystem :
    IEventListener<SpendResourcesEvent, IGuardPhaseEvent>,
    IEventListener<SpendResourcesEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 5;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<SpendResourcesEvent, IGuardPhaseEvent>.OnEvent(EventContext context, SpendResourcesEvent evt)
    {
        var entity = context.BattleState.GetEntity(evt.SpenderEntityId);
        if (entity == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"[ResourceCostSystem] Spender entity {evt.SpenderEntityId} not found. Cancelling.");
            return;
        }

        if (evt.ManaCost > 0)
        {
            var mana = entity.GetComponent<ManaComponent>();
            if (mana == null || mana.CurrentMana < evt.ManaCost)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"[ResourceCostSystem] Entity {entity.Id} has insufficient mana ({mana?.CurrentMana ?? 0}/{evt.ManaCost}). Cancelling.");
                return;
            }
        }

        if (evt.EnergyCost > 0)
        {
            var energy = entity.GetComponent<EnergyComponent>();
            if (energy == null || energy.CurrentEnergy < evt.EnergyCost)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"[ResourceCostSystem] Entity {entity.Id} has insufficient energy ({energy?.CurrentEnergy ?? 0}/{evt.EnergyCost}). Cancelling.");
                return;
            }
        }
    }

    void IEventListener<SpendResourcesEvent, IApplyPhaseEvent>.OnEvent(EventContext context, SpendResourcesEvent evt)
    {
        var entity = context.BattleState.GetEntity(evt.SpenderEntityId);
        if (entity == null) return;

        if (evt.ManaCost > 0)
        {
            var mana = entity.GetComponent<ManaComponent>();
            if (mana != null)
            {
                mana.CurrentMana -= evt.ManaCost;
                Debug.Log($"[ResourceCostSystem] Entity {entity.Id} spent {evt.ManaCost} mana. Remaining: {mana.CurrentMana}");
            }
        }

        if (evt.EnergyCost > 0)
        {
            var energy = entity.GetComponent<EnergyComponent>();
            if (energy != null)
            {
                energy.CurrentEnergy -= evt.EnergyCost;
                Debug.Log($"[ResourceCostSystem] Entity {entity.Id} spent {evt.EnergyCost} energy. Remaining: {energy.CurrentEnergy}");
            }
        }

        evt.Status = EventStatus.Applied;
    }
}
