using System;
using System.Collections.Generic;

public enum ComparisonOperator
{
    LessThan,
    LessThanOrEqual,
    Equal,
    GreaterThanOrEqual,
    GreaterThan
}

public class MetricLevelPredicate<TMetric> : IPredicate where TMetric : MetricComponent
{
    public int Threshold { get; set; }
    public ComparisonOperator Operator { get; set; }
    public Geid Subject { get; set; }

    public MetricLevelPredicate(ComparisonOperator comparisonOperator, int threshold, Geid subject)
    {
        Subject = subject;
        Threshold = threshold;
        Operator = comparisonOperator;
    }

    public bool Evaluate(EventContext eventContext)
    {
        var entity = eventContext.BattleState.GetEntity(Subject);
        if (entity == null) return false;
        if (!entity.HasComponent<TMetric>()) return false;
        var metric = entity.GetComponent<TMetric>();
        int currentValue = metric.Current;
        return Operator switch
        {
            ComparisonOperator.LessThan => currentValue < Threshold,
            ComparisonOperator.LessThanOrEqual => currentValue <= Threshold,
            ComparisonOperator.Equal => currentValue == Threshold,
            ComparisonOperator.GreaterThanOrEqual => currentValue >= Threshold,
            ComparisonOperator.GreaterThan => currentValue > Threshold,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

// Удобный wrapper для совместимости/удобства создания предикатов для здоровья
public class HealthLevelPredicate : MetricLevelPredicate<HealthComponent>
{
    public HealthLevelPredicate(ComparisonOperator comparisonOperator, int threshold, Geid subject)
        : base(comparisonOperator, threshold, subject)
    {
    }
}
public class ManaLevelPredicate : MetricLevelPredicate<ManaComponent>
{
    public ManaLevelPredicate(ComparisonOperator comparisonOperator, int threshold, Geid subject)
        : base(comparisonOperator, threshold, subject)
    {
    }
}

