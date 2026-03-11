using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Система исцеляющей молнии.
/// Apply-фаза: исцеляет цель, затем находит ближайшего не-исцелённого союзника
/// в радиусе 2 клеток. Если лечение / 2 >= 1 — создаёт следующее событие цепи.
/// </summary>
public class HealingLightningSystem : IEventListener<HealingLightningEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<HealingLightningEvent, IApplyPhaseEvent>.OnEvent(EventContext context, HealingLightningEvent evt)
    {
        var targetId = evt.GetFirstSubject(SubjectRole.Target);
        var target = context.BattleState.GetEntity(targetId);
        if (target == null)
        {
            evt.Status = EventStatus.Fizzled;
            return;
        }

        // Исцеляем цель
        var health = target.GetComponent<HealthComponent>();
        if (health != null)
        {
            int before = health.CurrentHealth;
            health.CurrentHealth = System.Math.Min(health.CurrentHealth + evt.HealAmount, health.MaxHealth);
            Debug.Log($"[HealingLightning] Entity {targetId} healed for {health.CurrentHealth - before}. HP: {health.CurrentHealth}/{health.MaxHealth}");
        }

        evt.Status = EventStatus.Applied;

        // Проверяем следующее звено цепи
        int nextHeal = evt.HealAmount / 2;
        if (nextHeal < 1) return;

        // Ищем ближайшего союзника в радиусе 2, ещё не задействованного в цепи
        var sourceId = evt.GetFirstSubject(SubjectRole.Source);
        var targetHex = target.GetComponent<HexComponent>();
        if (targetHex == null) return;

        Geid? nextTarget = FindNextChainTarget(context.BattleState, targetId, sourceId, targetHex.Coordinates, evt.AlreadyHealed);
        if (nextTarget == null) return;

        // Создаём следующее событие цепи
        var nextEvent = new HealingLightningEvent(
            evt.SystemSourceId,
            sourceId,
            nextTarget.Value,
            nextHeal,
            new List<Geid>(evt.AlreadyHealed)
        );
        context.Dispatcher.Enqueue(nextEvent);
    }

    private Geid? FindNextChainTarget(BattleState state, Geid currentTargetId, Geid sourceId, HexCoordinates center, List<Geid> alreadyHealed)
    {
        var sourceEntity = state.GetEntity(sourceId);
        var sourceTeam = sourceEntity?.GetComponent<TeamComponent>();
        if (sourceTeam == null) return null;

        Geid? nearest = null;
        int nearestDist = int.MaxValue;

        foreach (var entity in state.Entities.Values)
        {
            if (alreadyHealed.Contains(entity.Id)) continue;

            var team = entity.GetComponent<TeamComponent>();
            if (team == null || team.TeamId != sourceTeam.TeamId) continue;

            var hex = entity.GetComponent<HexComponent>();
            if (hex == null) continue;

            int dist = HexCoordinates.Distance(center, hex.Coordinates);
            if (dist > 2) continue;

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = entity.Id;
            }
        }
        return nearest;
    }
}
