public abstract class MetricComponent
{
    public int Current { get; set; }
    public int Max { get; set; }

    protected MetricComponent(int max)
    {
        Max = max;
        Current = max;
    }
}