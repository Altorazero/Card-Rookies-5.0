using System.Collections.Generic;

public class HealSelfCard : IPlayingCard
{
    public Geid Id { get; private set; } = Geid.New;
    public string Name { get; private set; } = "Heal Self";
    public string Description { get; private set; } = "Heals the player for a specified amount.";
    // public CardType Type { get; private set; }
    // public int ManaCost { get; private set; }
    public CardGraphNode CardGraphRootNode { get; private set; }
    
    public HealSelfCard(int healAmount)
    {
        Id = Geid.New;
        
        // Создаем спецификацию таргетинга для самоисцеления
        var targetingSpec = new BasicTargetingSpec
        {
            Description = "Target self",
            Type = TargetingType.Entity,
            TargetFilter = new SelfTargetFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target
        };
        
        // Создаем событие исцеления с таргетингом
        var healEvent = new HealEventWithTargeting(Id, Id, Geid.Empty, healAmount, targetingSpec);
        
        // Создаем корневой узел графа карты
        CardGraphRootNode = new CardGraphNode(new List<IGameEvent> { healEvent });
        
        // Добавляем условие: требуется минимум 2 маны
        var manaCondition = new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, 2, Geid.Empty);
        
        // Можно добавить дополнительные узлы при необходимости
        // CardGraphRootNode.TieNode(nextNode, manaCondition);
    }
}