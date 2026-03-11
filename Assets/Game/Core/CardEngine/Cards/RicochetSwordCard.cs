using System.Collections.Generic;

/// <summary>
/// Карта «Меч-рикошет».
/// Запускает меч в выбранного противника, нанося 4 урона.
/// После попадания может рикошетить к ближайшему противнику в радиусе 3 клеток,
/// тратя 1 ману и 1 энергию за каждый рикошет (останавливается при нехватке ресурсов).
/// Начальная стоимость: 1 мана и 3 энергии.
/// - Ресурсов нет — cancelled.
/// - Нет начальной цели — fizzled.
/// </summary>
public class RicochetSwordCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Меч-рикошет";
    public string Description => "Меч наносит 4 урона противнику и рикошетит (1 мана + 1 энергия за рикошет).";
    public int ManaCost => 1;
    public int EnergyCost => 3;
    public CardGraphNode CardGraphRootNode { get; }

    /// <param name="casterEntityId">Кастующая сущность.</param>
    /// <param name="initialTargetId">Первоначальная цель.</param>
    public RicochetSwordCard(Geid casterEntityId, Geid initialTargetId)
    {
        // Событие траты ресурсов
        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);

        // Первый удар через RicochetSwordEvent
        var ricochetEvent = new RicochetSwordEvent(casterEntityId, casterEntityId, initialTargetId);

        var effectNode = new CardGraphNode(new List<IGameEvent> { spendEvent, ricochetEvent });
        var rootNode = new CardGraphNode(new List<IGameEvent>());

        var hasResources = CompositePredicates.And(
            new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, ManaCost, casterEntityId),
            new EnergyLevelPredicate(ComparisonOperator.GreaterThanOrEqual, EnergyCost, casterEntityId)
        );
        rootNode.TieNode(effectNode, hasResources);

        CardGraphRootNode = rootNode;
    }
}
