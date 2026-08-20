/// <summary>
/// Компонент энергии сущности.
/// Энергия используется как ресурс для разыгрывания карт и применения способностей.
/// </summary>
public record EnergyComponent(int Current, int Max) : IComponent;

