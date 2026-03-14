using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Состояние цикла «Исцеляющей молнии».
/// Хранит текущую цель, текущую силу лечения и список уже посещённых целей.
/// На каждой итерации исцеляет текущую цель, затем ищет ближайшего союзника
/// в радиусе 2 клеток и делит силу лечения на 2 (округление вниз).
/// Цикл заканчивается, когда сила лечения &lt; 1 или новая цель не найдена.
/// </summary>
public class HealingLightningLoopState : ILoopState
{
    private const int ChainRadius = 2;

    private readonly Geid _sourceId;
    private readonly Geid _currentTargetId;
    private readonly int _currentHealAmount;
    private readonly IReadOnlyList<Geid> _visited;

    public HealingLightningLoopState(Geid sourceId, Geid targetId, int healAmount, List<Geid> visited)
    {
        _sourceId = sourceId;
        _currentTargetId = targetId;
        _currentHealAmount = healAmount;
        _visited = visited.AsReadOnly();
    }

    public bool ShouldContinue(EventContext context) =>
        _currentHealAmount >= 1 && context.BattleState.GetEntity(_currentTargetId) != null;

    public IGameEvent CreateStepEffect(EventContext context) =>
        new HealEvent(_sourceId, _sourceId, _currentTargetId, _currentHealAmount);

    public ILoopState Advance(EventContext context)
    {
        int nextHeal = _currentHealAmount / 2;
        if (nextHeal < 1)
            return null;

        var target = context.BattleState.GetEntity(_currentTargetId);
        var source = context.BattleState.GetEntity(_sourceId);
        if (target == null || source == null)
            return null;

        var targetHex = target.GetComponent<HexComponent>();
        var sourceTeam = source.GetComponent<TeamComponent>();
        if (targetHex == null || sourceTeam == null)
            return null;

        var newVisited = new List<Geid>(_visited) { _currentTargetId };
        Geid? nextTarget = FindNextTarget(context.BattleState, targetHex.Coordinates, sourceTeam.TeamId, newVisited);
        if (nextTarget == null)
            return null;

        return new HealingLightningLoopState(_sourceId, nextTarget.Value, nextHeal, newVisited);
    }

    private static Geid? FindNextTarget(BattleState state, HexCoordinates center, Geid teamId, List<Geid> visited)
    {
        Geid? nearest = null;
        int nearestDist = int.MaxValue;

        foreach (var entity in state.Entities.Values)
        {
            if (visited.Contains(entity.Id))
                continue;

            var team = entity.GetComponent<TeamComponent>();
            if (team == null || team.TeamId != teamId)
                continue;

            var hex = entity.GetComponent<HexComponent>();
            if (hex == null)
                continue;

            int dist = HexCoordinates.Distance(center, hex.Coordinates);
            if (dist > ChainRadius || dist >= nearestDist)
                continue;

            nearestDist = dist;
            nearest = entity.Id;
        }

        return nearest;
    }
}
