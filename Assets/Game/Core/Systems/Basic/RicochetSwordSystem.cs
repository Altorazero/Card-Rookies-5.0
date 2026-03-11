using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система рикошета меча.
/// Apply-фаза: наносит 4 урона текущей цели, затем проверяет условия рикошета:
/// - У кастера есть 1 мана и 1 энергия
/// - Есть другой не-поражённый противник в радиусе 3 клеток от текущей цели
/// Если условия выполнены — тратит ресурсы и создаёт следующий RicochetSwordEvent.
/// </summary>
public class RicochetSwordSystem : IEventListener<RicochetSwordEvent, IApplyPhaseEvent>
{
    private const int RicochetDamage = 4;
    private const int RicochetManaCost = 1;
    private const int RicochetEnergyCost = 1;
    private const int RicochetRadius = 3;

    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<RicochetSwordEvent, IApplyPhaseEvent>.OnEvent(EventContext context, RicochetSwordEvent evt)
    {
        var targetId = evt.GetFirstSubject(SubjectRole.Target);
        var target = context.BattleState.GetEntity(targetId);
        if (target == null)
        {
            evt.Status = EventStatus.Fizzled;
            return;
        }

        // Наносим урон
        context.Dispatcher.Enqueue(new SingleDamageEvent(
            evt.SystemSourceId,
            evt.CasterEntityId,
            targetId,
            RicochetDamage,
            DamageType.Physical
        ));

        evt.Status = EventStatus.Applied;

        // Проверяем условия следующего рикошета
        var caster = context.BattleState.GetEntity(evt.CasterEntityId);
        if (caster == null) return;

        var mana = caster.GetComponent<ManaComponent>();
        var energy = caster.GetComponent<EnergyComponent>();

        bool hasMana = mana != null && mana.CurrentMana >= RicochetManaCost;
        bool hasEnergy = energy != null && energy.CurrentEnergy >= RicochetEnergyCost;

        if (!hasMana || !hasEnergy) return;

        // Ищем следующую цель — противник в радиусе 3 от текущей цели, не поражённый ранее
        var targetHex = target.GetComponent<HexComponent>();
        if (targetHex == null) return;

        Geid? nextTarget = FindRicochetTarget(context.BattleState, evt.CasterEntityId, targetHex.Coordinates, evt.AlreadyHit);
        if (nextTarget == null) return;

        // Тратим ресурсы
        if (mana != null) mana.CurrentMana -= RicochetManaCost;
        if (energy != null) energy.CurrentEnergy -= RicochetEnergyCost;

        Debug.Log($"[RicochetSword] Ricocheting to {nextTarget}. Caster mana: {mana?.CurrentMana}, energy: {energy?.CurrentEnergy}");

        // Создаём следующий рикошет
        var nextRicochet = new RicochetSwordEvent(
            evt.SystemSourceId,
            evt.CasterEntityId,
            nextTarget.Value,
            new List<Geid>(evt.AlreadyHit)
        );
        context.Dispatcher.Enqueue(nextRicochet);
    }

    private Geid? FindRicochetTarget(BattleState state, Geid casterId, HexCoordinates fromHex, List<Geid> alreadyHit)
    {
        var caster = state.GetEntity(casterId);
        var casterTeam = caster?.GetComponent<TeamComponent>();
        if (casterTeam == null) return null;

        Geid? nearest = null;
        int nearestDist = int.MaxValue;

        foreach (var entity in state.Entities.Values)
        {
            if (alreadyHit.Contains(entity.Id)) continue;

            var team = entity.GetComponent<TeamComponent>();
            if (team == null || team.TeamId == casterTeam.TeamId) continue; // Пропускаем союзников

            var hex = entity.GetComponent<HexComponent>();
            if (hex == null) continue;

            int dist = HexCoordinates.Distance(fromHex, hex.Coordinates);
            if (dist > RicochetRadius) continue;

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = entity.Id;
            }
        }
        return nearest;
    }
}
