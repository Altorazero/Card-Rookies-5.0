using System.Collections.Generic;

/// <summary>
/// Карта «Исцеляющая молния».
/// Выбранный союзник исцеляется на 4 хп. Молния переходит к ближайшему союзнику
/// в радиусе 2 клеток с уполовиненным лечением (округление вниз). Цепь продолжается
/// пока lечение >= 1.
/// Нет стоимости (бесплатная), выбор цели — конкретный союзник.
/// </summary>
public class HealingLightningCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Исцеляющая молния";
    public string Description => "Исцеляет союзника на 4 хп, молния переходит к ближайшему союзнику (радиус 2, лечение делится на 2).";
    public int ManaCost => 0;
    public int EnergyCost => 0;
    public CardGraphNode CardGraphRootNode { get; }

    /// <param name="casterEntityId">Кастующая сущность.</param>
    /// <param name="initialTargetId">Первичная цель (союзник, которому будет применено лечение).</param>
    public HealingLightningCard(Geid casterEntityId, Geid initialTargetId)
    {
        // Событие первого исцеления (цепь обрабатывается HealingLightningSystem)
        var lightningEvent = new HealingLightningEvent(casterEntityId, casterEntityId, initialTargetId, 4);

        CardGraphRootNode = new CardGraphNode(new List<IGameEvent> { lightningEvent });
    }
}
