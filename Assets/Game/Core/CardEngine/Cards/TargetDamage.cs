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
        
        // ������� ������������ ���������� ��� �������������
        var targetingSpec = new TargetingSpec { Description = "Target self", TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new SelfTargetFilter()))
            .AddStep(new TakeSorter(1))
            .AddStep(new ExitConditionStep(new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));
        
        // ������� ������� ��������� � �����������
        var healEvent = new HealEventWithTargeting(Id, Id, Geid.Empty, healAmount, targetingSpec);
        
        // ������� �������� ���� ����� �����
        CardGraphRootNode = new CardGraphNode(new List<IGameEvent> { healEvent });
        
        // ��������� �������: ��������� ������� 2 ����
        var manaCondition = new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, 2, Geid.Empty);
        
        // ����� �������� �������������� ���� ��� �������������
        // CardGraphRootNode.TieNode(nextNode, manaCondition);
    }
}