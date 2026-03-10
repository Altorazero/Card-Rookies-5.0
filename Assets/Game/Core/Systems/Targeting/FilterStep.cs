using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Шаг пайплайна: фильтрует кандидатов с помощью <see cref="ITargetFilter"/>.
/// Кандидаты, не прошедшие фильтр, удаляются из <see cref="TargetingContext.Candidates"/>.
///
/// Поскольку <see cref="ITargetFilter"/> поддерживает полноценную компоновку
/// (<see cref="AndTargetFilter"/>, <see cref="OrTargetFilter"/>, <see cref="NotTargetFilter"/>),
/// один FilterStep может реализовывать произвольно сложную логику фильтрации.
/// </summary>
public class FilterStep : ITargetingStep
{
    public ITargetFilter Filter { get; }

    public FilterStep(ITargetFilter filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public void Execute(TargetingContext context)
    {
        var valid = new List<Geid>(context.Candidates.Count);

        foreach (var candidate in context.Candidates)
        {
            context.EventContext.EvaluatingCandidate = candidate;
            bool passes = false;

            try
            {
                passes = Filter.IsTargetValid(candidate, context.EventContext);
            }
            catch (Exception ex)
            {
        Debug.LogWarning($"[FilterStep] Error filtering candidate {candidate}: {ex.Message}");
            }

            if (passes)
                valid.Add(candidate);
        }

        context.EventContext.EvaluatingCandidate = null;
        context.Candidates.Clear();
        context.Candidates.AddRange(valid);
    }
}
