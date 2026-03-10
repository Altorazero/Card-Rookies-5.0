using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система таргетинга. Исполняет пайплайн шагов из <see cref="ITargetingSpec"/>
/// и записывает найденные цели в Subjects события во время фазы TargetResolve.
///
/// Зона ответственности:
///   – последовательно выполнить шаги пайплайна;
///   – после завершения пайплайна записать кандидатов в Subjects, если они ещё не записаны
///     и статус события не был изменён.
///
/// За пределами ответственности:
///   – логика сбора кандидатов       → пул-шаги (AllEntitiesPool, ExplicitEntitiesPool, …);
///   – логика фильтрации             → FilterStep + ITargetFilter (And/Or/Not/…);
///   – логика сортировки/ограничения → HighestHpSorter, LowestHpSorter, NearestSorter, TakeSorter, …;
///   – обработка ошибок и альтернативных эффектов → ExitConditionStep + ITargetingAction.
/// </summary>
public class TargetingSystem : IEventListener<INeedTargeting, ITargetResolvePhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<INeedTargeting, ITargetResolvePhaseEvent>.OnEvent(EventContext context, INeedTargeting evt)
    {
        if (evt.TargetingSpec == null)
        {
            Debug.LogWarning($"[TargetingSystem] Event {evt.Id} has no TargetingSpec. Skipping.");
            return;
        }

        ResolveTargeting(context, evt);
    }

    private void ResolveTargeting(EventContext context, INeedTargeting evt)
    {
        var spec = evt.TargetingSpec;
        var pipeline = new TargetingContext(context, evt, spec);

        foreach (var step in spec.Steps)
        {
            if (pipeline.Stopped) break;
            step.Execute(pipeline);
        }

        // Если пайплайн завершился без явной фиксации целей и событие всё ещё активно —
        // фиксируем кандидатов как цели.
        if (!pipeline.AlreadyCommitted && evt.Status == EventStatus.Pending)
            PopulateTargets(evt, spec, pipeline.Candidates);
    }

    private void PopulateTargets(INeedTargeting evt, ITargetingSpec spec, IReadOnlyList<Geid> targets)
    {
        int roleIdx = (int)spec.TargetRole;
        evt.EnsureSubjects();
        while (evt.Subjects.Count <= roleIdx)
            evt.Subjects.Add(new List<Geid>());

        foreach (var id in targets)
            evt.Subjects[roleIdx].Add(id);
    }
}
