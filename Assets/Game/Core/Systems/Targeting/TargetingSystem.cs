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

        // Получаем кандидатов для таргетинга
        var candidates = GetCandidates(context, spec);

        // Фильтруем кандидатов
        var validCandidates = FilterCandidates(context, candidates, spec);

        // Выбираем цели с помощью селектора
        IReadOnlyList<Geid> selectedTargets;
        
        if (spec.Selector != null)
        {
            selectedTargets = spec.Selector.SelectTarget(context, validCandidates);
        }
        else
        {
            // Если селектор не указан, берем все валидные цели в пределах MaxTargets
            selectedTargets = validCandidates.Take(spec.MaxTargets).ToList();
        }

        // Проверяем минимальное количество целей
        if (selectedTargets.Count < spec.MinTargets)
        {
            throw new TargetSelectionFailed(spec.Id, spec.MinTargets, selectedTargets.Count);
        }

        // Добавляем выбранные цели в SubjectsList
        if (evt.SubjectsList == null)
        {
            evt.SubjectsList = new List<Subject>();
        }

        foreach (var targetId in selectedTargets)
        {
            evt.SubjectsList.Add(new Subject
            {
                Entity = targetId,
                Role = spec.TargetRole
            });
        }
    }

    private List<Geid> GetCandidates(EventContext context, ITargetingSpec spec)
    {
        var candidates = new List<Geid>();

        switch (spec.Type)
        {
            case TargetingType.Entity:
                // Получаем все сущности на поле боя
                candidates.AddRange(context.BattleState.Entities.Keys);
                break;

            case TargetingType.Area:
                // Для области можно добавить логику получения сущностей в радиусе
                // Пока используем все сущности
                candidates.AddRange(context.BattleState.Entities.Keys);
                break;

            case TargetingType.Direction:
            case TargetingType.Projectile:
                // Для направления и снарядов - пока все сущности
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
        {
            return candidates;
        }

        var validCandidates = new List<Geid>();

        foreach (var candidate in candidates)
        {
            // Устанавливаем текущего кандидата для оценки
            context.EvaluatingCandidate = candidate;

            try
            {
                if (spec.TargetFilter.IsTargetValid(candidate, context))
                {
                    validCandidates.Add(candidate);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error filtering candidate {candidate}: {ex.Message}");
            }
        }

        // Сбрасываем кандидата после фильтрации
        context.EvaluatingCandidate = null;

        return validCandidates;
    }
}