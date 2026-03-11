/// <summary>
/// Компонент горения сущности.
/// При наличии этого компонента сущность получает урон от огня в начале своего хода.
/// </summary>
public class BurnComponent
{
    /// <summary>Урон огня за один тик (начало хода).</summary>
    public int DamagePerTick { get; set; }

    /// <summary>Оставшееся количество тиков.</summary>
    public int RemainingTicks { get; set; }

    public BurnComponent(int damagePerTick, int ticks)
    {
        DamagePerTick = damagePerTick;
        RemainingTicks = ticks;
    }
}
