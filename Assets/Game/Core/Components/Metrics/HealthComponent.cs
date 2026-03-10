public class HealthComponent : MetricComponent
{
    public int CurrentHealth { get => Current; set => Current = value; }
    public int MaxHealth { get => Max; set => Max = value; }

    public HealthComponent(int maxHealth) : base(maxHealth)
    {
    }
}