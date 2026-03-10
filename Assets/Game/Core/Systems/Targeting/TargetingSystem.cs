using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Система таргетинга. По спецификации из <see cref="INeedTargeting"/> находит цели
/// и помещает их в Subjects события во время фазы TargetResolve.
///
/// Зона ответственности:
///   – получить пул кандидатов (все сущности BattleState по умолчанию);
///   – применить фильтры (AND-логика);
///   – отсортировать / выбрать финальный список по Priority и MaxTargets;
///   – проверить MinTargets и выполнить указанное поведение при нехватке.
///
/// За пределами ответственности:
///   – реализация альтернативных эффектов: система лишь вызывает
///     AlternativeEffectFactory из спека и ставит результат в очередь.
/// </summary>
public class TargetingSystem : IEventListener<INeedTargeting, ITargetResolvePhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<INeedTargeting, ITargetResolvePhaseEvent>.OnEvent(EventContext context, INeedTargeting evt)
    {
        if (evt.TargetingSpec == null)
        {
            Debug.LogWarning($"[TargetingSystem] Event {evt.Id} has no targeting spec. Skipping.");
            return;
        }

        ResolveTargeting(context, evt);
    }

    private void ResolveTargeting(EventContext context, INeedTargeting evt)
    {
        var spec = evt.TargetingSpec;

        var candidates = GetCandidates(context, spec);
        var validCandidates = FilterCandidates(context, candidates, spec);
        var selectedTargets = SelectTargets(context, validCandidates, spec);

        if (selectedTargets.Count < spec.MinTargets)
        {
            HandleInsufficientTargets(context, evt, spec, selectedTargets);
            return;
        }

        PopulateTargets(evt, spec, selectedTargets);
    }

    // -----------------------------------------------------------------------
    // Candidate collection
    // -----------------------------------------------------------------------

    private List<Geid> GetCandidates(EventContext context, ITargetingSpec spec)
    {
        return spec.Type switch
        {
            TargetingType.None => new List<Geid>(),
            _ => context.BattleState.Entities.Keys.ToList(),
        };
    }

    // -----------------------------------------------------------------------
    // Filtering  (AND logic across all filters)
    // -----------------------------------------------------------------------

    private List<Geid> FilterCandidates(EventContext context, List<Geid> candidates, ITargetingSpec spec)
    {
        if (spec.Filters == null || spec.Filters.Count == 0)
            return candidates;

        var valid = new List<Geid>();

        foreach (var candidate in candidates)
        {
            context.EvaluatingCandidate = candidate;
            bool passes = true;

            try
            {
                foreach (var filter in spec.Filters)
                {
                    if (!filter.IsTargetValid(candidate, context))
                    {
                        passes = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TargetingSystem] Filter error for candidate {candidate}: {ex.Message}");
                passes = false;
            }

            if (passes)
                valid.Add(candidate);
        }

        context.EvaluatingCandidate = null;
        return valid;
    }

    // -----------------------------------------------------------------------
    // Selection  (sort by priority, then take up to MaxTargets)
    // -----------------------------------------------------------------------

    private IReadOnlyList<Geid> SelectTargets(EventContext context, List<Geid> candidates, ITargetingSpec spec)
    {
        if (candidates.Count == 0)
            return new List<Geid>();

        IEnumerable<Geid> ordered = OrderCandidates(context, candidates, spec);

        int count = spec.MaxTargets == TargetCount.All ? candidates.Count : spec.MaxTargets;
        return ordered.Take(count).ToList();
    }

    private IEnumerable<Geid> OrderCandidates(EventContext context, List<Geid> candidates, ITargetingSpec spec)
    {
        switch (spec.Priority)
        {
            case TargetPriority.Random:
                // Используем BattleState.Rng для воспроизводимости при одном seed
                return candidates.OrderBy(_ => context.BattleState.Rng.NextInt(int.MaxValue));

            case TargetPriority.HighestHp:
                return candidates.OrderByDescending(id =>
                {
                    var e = context.BattleState.GetEntity(id);
                    return e?.GetComponent<HealthComponent>()?.Current ?? 0;
                });

            case TargetPriority.LowestHp:
                return candidates.OrderBy(id =>
                {
                    var e = context.BattleState.GetEntity(id);
                    return e?.GetComponent<HealthComponent>()?.Current ?? int.MaxValue;
                });

            case TargetPriority.Nearest:
                return OrderByNearest(context, candidates, spec);

            case TargetPriority.First:
            default:
                return candidates;
        }
    }

    private IEnumerable<Geid> OrderByNearest(EventContext context, List<Geid> candidates, ITargetingSpec spec)
    {
        if (spec.SourceEntity == null)
            return candidates;

        var sourceEntity = context.BattleState.GetEntity(spec.SourceEntity.Value);
        var sourceHex = sourceEntity?.GetComponent<HexComponent>();

        if (sourceHex == null)
            return candidates;

        return candidates.OrderBy(id =>
        {
            var e = context.BattleState.GetEntity(id);
            var hex = e?.GetComponent<HexComponent>();
            return hex == null ? int.MaxValue : HexCoordinates.Distance(sourceHex.Coordinates, hex.Coordinates);
        });
    }

    // -----------------------------------------------------------------------
    // Insufficient-targets fallback
    // -----------------------------------------------------------------------

    private void HandleInsufficientTargets(
        EventContext context,
        INeedTargeting evt,
        ITargetingSpec spec,
        IReadOnlyList<Geid> foundTargets)
    {
        switch (spec.OnInsufficientTargets)
        {
            case InsufficientTargetsBehavior.Cancel:
                Debug.LogWarning(
                    $"[TargetingSystem] Event {evt.Id}: insufficient targets " +
                    $"(required {spec.MinTargets}, found {foundTargets.Count}). Fizzling.");
                evt.Status = EventStatus.Fizzled;
                break;

            case InsufficientTargetsBehavior.UseFound:
                // Продолжаем с найденными целями (может быть 0)
                PopulateTargets(evt, spec, foundTargets);
                break;

            case InsufficientTargetsBehavior.ShootVoid:
                // Намеренный выстрел в пустоту: Subjects не заполняются, событие продолжается
                Debug.Log(
                    $"[TargetingSystem] Event {evt.Id}: shooting into void " +
                    $"(required {spec.MinTargets}, found {foundTargets.Count}).");
                break;

            case InsufficientTargetsBehavior.AlternativeEffect:
                evt.Status = EventStatus.Cancelled;
                if (spec.AlternativeEffectFactory != null)
                {
                    var altEvent = spec.AlternativeEffectFactory(context);
                    if (altEvent != null)
                    {
                        Debug.Log(
                            $"[TargetingSystem] Event {evt.Id}: dispatching alternative event {altEvent.Id}.");
                        context.Dispatcher.Enqueue(altEvent, atFront: true);
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"[TargetingSystem] Event {evt.Id}: AlternativeEffect behavior " +
                        "specified but AlternativeEffectFactory is null.");
                }
                break;

            default:
                Debug.LogWarning(
                    $"[TargetingSystem] Unknown InsufficientTargetsBehavior: {spec.OnInsufficientTargets}");
                evt.Status = EventStatus.Fizzled;
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Target population
    // -----------------------------------------------------------------------

    private void PopulateTargets(INeedTargeting evt, ITargetingSpec spec, IReadOnlyList<Geid> targets)
    {
        evt.EnsureSubjects();
        int roleIdx = (int)spec.TargetRole;
        while (evt.Subjects.Count <= roleIdx)
            evt.Subjects.Add(new List<Geid>());

        foreach (var targetId in targets)
            evt.Subjects[roleIdx].Add(targetId);
    }
}
