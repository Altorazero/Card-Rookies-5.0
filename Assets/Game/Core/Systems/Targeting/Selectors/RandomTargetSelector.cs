using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ¬ыбирает случайные цели из кандидатов
/// </summary>
public class RandomTargetSelector : ITargetSelector
{
    public int Count { get; set; }

    public RandomTargetSelector(int count = 1)
    {
        Count = count;
    }

    public IReadOnlyList<Geid> SelectTarget(EventContext context, IReadOnlyList<Geid> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return new List<Geid>();
        }

        var random = new System.Random();
        return candidates.OrderBy(x => random.Next()).Take(Count).ToList();
    }
}