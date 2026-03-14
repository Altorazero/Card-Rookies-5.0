using System.Collections.Generic;

/// <summary>
/// Карта «Меч-рикошет».
/// Запускает меч в выбранного противника, нанося 4 урона.
/// После попадания может рикошетить к ближайшему противнику в радиусе 3 клеток,
/// тратя 1 ману и 1 энергию за каждый рикошет (останавливается при нехватке ресурсов).
/// Начальная стоимость: 1 мана и 3 энергии.
/// - Ресурсов нет — SpendResourcesEvent отменяется.
/// - Нет начальной цели — fizzled.
/// </summary>
public class RicochetSwordCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Меч-рикошет";
    public string Description => "Меч наносит 4 урона противнику и рикошетит (1 мана + 1 энергия за рикошет).";
    public int ManaCost => 1;
    public int EnergyCost => 3;
    public IReadOnlyList<IGameEvent> Effects { get; }

    /// <param name="casterEntityId">Кастующая сущность.</param>
    /// <param name="initialTargetId">Первоначальная цель.</param>
    public RicochetSwordCard(Geid casterEntityId, Geid initialTargetId)
    {
        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);
        var ricochetEvent = new RicochetSwordEvent(casterEntityId, casterEntityId, initialTargetId);

        Effects = new List<IGameEvent> { spendEvent, ricochetEvent };
    }
}
