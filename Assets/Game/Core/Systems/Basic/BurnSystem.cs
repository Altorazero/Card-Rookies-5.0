using UnityEngine;

/// <summary>
/// Система горения. 
/// Apply-фаза ApplyBurnEvent: накладывает или усиливает BurnComponent на цель.
/// TurnStartEvent: наносит урон горящим сущностям команды, чей ход начался.
/// </summary>
public class BurnSystem :
    IEventListener<ApplyBurnEvent, IApplyPhaseEvent>,
    IEventListener<TurnStartEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 150;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<ApplyBurnEvent, IApplyPhaseEvent>.OnEvent(EventContext context, ApplyBurnEvent evt)
    {
        var targetId = evt.GetFirstSubject(SubjectRole.Target);
        var target = context.BattleState.GetEntity(targetId);
        if (target == null)
        {
            evt.Status = EventStatus.Fizzled;
            return;
        }

        var burn = target.GetComponent<BurnComponent>();
        if (burn != null)
        {
            // Усиливаем существующий эффект горения
            burn.DamagePerTick = System.Math.Max(burn.DamagePerTick, evt.DamagePerTick);
            burn.RemainingTicks += evt.Ticks;
        }
        else
        {
            target.AddComponent(new BurnComponent(evt.DamagePerTick, evt.Ticks));
        }

        Debug.Log($"[BurnSystem] Entity {targetId} is now burning ({evt.DamagePerTick} dmg/tick).");
        evt.Status = EventStatus.Applied;
    }

    void IEventListener<TurnStartEvent, IApplyPhaseEvent>.OnEvent(EventContext context, TurnStartEvent evt)
    {
        var teamId = evt.TeamId;

        foreach (var entity in context.BattleState.Entities.Values)
        {
            var team = entity.GetComponent<TeamComponent>();
            if (team == null || team.TeamId != teamId) continue;

            var burn = entity.GetComponent<BurnComponent>();
            if (burn == null || burn.RemainingTicks <= 0) continue;

            // Наносим урон от горения
            var health = entity.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.CurrentHealth -= burn.DamagePerTick;
                Debug.Log($"[BurnSystem] Entity {entity.Id} took {burn.DamagePerTick} burn damage. HP: {health.CurrentHealth}");
            }

            burn.RemainingTicks--;
            if (burn.RemainingTicks <= 0)
            {
                burn.RemainingTicks = 0;
                Debug.Log($"[BurnSystem] Entity {entity.Id} burn expired.");
            }
        }
    }
}
