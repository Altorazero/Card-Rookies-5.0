using System.Collections.Generic;

/// <summary>
/// Выбирает все доступные цели
/// </summary>
public class AllTargetsSelector : ITargetSelector
{
    public IReadOnlyList<Geid> SelectTarget(EventContext context, IReadOnlyList<Geid> candidates)
    {
        return candidates ?? new List<Geid>();
    }
}