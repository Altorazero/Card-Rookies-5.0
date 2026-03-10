using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetingSystem : IEventListener<INeedTargeting, ITargetResolvePhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<INeedTargeting, ITargetResolvePhaseEvent>.OnEvent(EventContext context, INeedTargeting evt)
    {
        if (evt.TargetingSpec == null)
        {
            Debug.LogWarning($"Event {evt.Id} has no targeting spec. Skipping targeting.");
            return;
        }

        try
        {
            ResolveTargeting(context, evt);
        }
        catch (TargetSelectionFailed ex)
        {
            Debug.LogWarning($"Targeting failed for event {evt.Id}: {ex.Message}");
            evt.Status = EventStatus.Fizzled;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error during targeting for event {evt.Id}: {ex}");
            evt.Status = EventStatus.Fizzled;
        }
    }

    private void ResolveTargeting(EventContext context, INeedTargeting evt)
    {
        var spec = evt.TargetingSpec;

        var candidates = GetCandidates(context, spec);
        var validCandidates = FilterCandidates(context, candidates, spec);

        IReadOnlyList<Geid> selectedTargets;
        if (spec.Selector != null)
            selectedTargets = spec.Selector.SelectTarget(context, validCandidates);
        else
            selectedTargets = validCandidates.Take(spec.MaxTargets).ToList();

        if (selectedTargets.Count < spec.MinTargets)
            throw new TargetSelectionFailed(spec.Id, spec.MinTargets, selectedTargets.Count);

        // Добавляем выбранные цели в Subjects по указанной роли
        evt.EnsureSubjects();
        int roleIdx = (int)spec.TargetRole;
        while (evt.Subjects.Count <= roleIdx)
            evt.Subjects.Add(new List<Geid>());

        foreach (var targetId in selectedTargets)
            evt.Subjects[roleIdx].Add(targetId);
    }

    private List<Geid> GetCandidates(EventContext context, ITargetingSpec spec)
    {
        var candidates = new List<Geid>();

        switch (spec.Type)
        {
            case TargetingType.Entity:
            case TargetingType.Area:
            case TargetingType.Direction:
            case TargetingType.Projectile:
                candidates.AddRange(context.BattleState.Entities.Keys);
                break;

            case TargetingType.None:
            default:
                break;
        }

        return candidates;
    }

    private List<Geid> FilterCandidates(EventContext context, List<Geid> candidates, ITargetingSpec spec)
    {
        if (spec.TargetFilter == null)
            return candidates;

        var validCandidates = new List<Geid>();

        foreach (var candidate in candidates)
        {
            context.EvaluatingCandidate = candidate;
            try
            {
                if (spec.TargetFilter.IsTargetValid(candidate, context))
                    validCandidates.Add(candidate);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error filtering candidate {candidate}: {ex.Message}");
            }
        }

        context.EvaluatingCandidate = null;
        return validCandidates;
    }
}
