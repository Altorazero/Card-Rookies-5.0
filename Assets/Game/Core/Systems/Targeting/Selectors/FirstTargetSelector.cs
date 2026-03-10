using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ¬ыбирает первые N целей из кандидатов
/// </summary>
public class FirstTargetSelector : ITargetSelector
{
    public int Count { get; set; }

    public FirstTargetSelector(int count = 1)
    {
        Count = count;
    }

    public IReadOnlyList<Geid> SelectTarget(EventContext context, IReadOnlyList<Geid> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return new List<Geid>();
        }

        return candidates.Take(Count).ToList();
    }
}