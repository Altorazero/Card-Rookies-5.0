/// <summary>
/// Компонент энергии сущности.
/// Энергия используется как ресурс для разыгрывания карт и применения способностей.
/// </summary>
public class EnergyComponent : MetricComponent
{
    public int CurrentEnergy { get => Current; set => Current = value; }
    public int MaxEnergy { get => Max; set => Max = value; }

    public EnergyComponent(int maxEnergy) : base(maxEnergy)
    {
    }
}
