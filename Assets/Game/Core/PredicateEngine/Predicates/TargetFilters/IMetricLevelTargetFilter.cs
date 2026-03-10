public class IMetricLevelTargetFilter<TMetric> : ITargetFilter where TMetric : MetricComponent
{
    public int Threshold { get; set; }
    public ComparisonOperator Operator { get; set; }
    public IMetricLevelTargetFilter(ComparisonOperator comparisonOperator, int threshold)
    {
        Threshold = threshold;
        Operator = comparisonOperator;
    }
    public bool IsTargetValid(Geid target, EventContext context)
    {
        var innerPredicate = new MetricLevelPredicate<TMetric>(Operator, Threshold, target);
        return innerPredicate.Evaluate(context);
    }
}