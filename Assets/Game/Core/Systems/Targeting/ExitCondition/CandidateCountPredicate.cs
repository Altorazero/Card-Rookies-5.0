using System;
using System.Collections.Generic;

/// <summary>
/// Предикат: сравнивает количество текущих кандидатов с порогом.
///
/// Пример — «найдено хотя бы 1»:
/// <code>new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1)</code>
///
/// Пример — «найдено меньше 3»:
/// <code>new CandidateCountPredicate(ComparisonOperator.LessThan, 3)</code>
/// </summary>
public class CandidateCountPredicate : ITargetListPredicate
{
    public ComparisonOperator Operator { get; }
    public int Threshold { get; }

    public CandidateCountPredicate(ComparisonOperator @operator, int threshold)
    {
        Operator = @operator;
        Threshold = threshold;
    }

    public bool Evaluate(IReadOnlyList<Geid> candidates, EventContext context)
    {
        int count = candidates.Count;
        return Operator switch
        {
            ComparisonOperator.LessThan           => count < Threshold,
            ComparisonOperator.LessThanOrEqual    => count <= Threshold,
            ComparisonOperator.Equal              => count == Threshold,
            ComparisonOperator.GreaterThanOrEqual => count >= Threshold,
            ComparisonOperator.GreaterThan        => count > Threshold,
            _ => throw new ArgumentOutOfRangeException(nameof(Operator), Operator, null)
        };
    }
}
