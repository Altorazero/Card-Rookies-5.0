public class ManaComponent : MetricComponent
{
    public int CurrentMana { get => Current; set => Current = value; }
    public int MaxMana { get => Max; set => Max = value; }

    public ManaComponent(int maxMana) : base(maxMana)
    {
    }
}