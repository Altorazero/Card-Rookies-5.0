using System.Collections.Generic;

public class HealSelfCard : IPlayingCard
{
    public Geid Id { get; private set; } = Geid.New;
    public string Name { get; private set; } = "Heal Self";
    public string Description { get; private set; } = "Heals the player for a specified amount.";
    public int ManaCost { get; private set; }
    public int EnergyCost { get; private set; }
    public IReadOnlyList<IGameEvent> Effects { get; private set; }

    public HealSelfCard(int healAmount, int manaCost = 2)
    {
        Id = Geid.New;
        ManaCost = manaCost;
        EnergyCost = 0;

        // Создаём спецификацию таргетинга для самоисцеления
        var targetingSpec = new TargetingSpec { Description = "Target self", TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new SelfTargetFilter()))
            .AddStep(new TakeSorter(1))
            .AddStep(new ExitConditionStep(new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));

        var healEvent = new HealEventWithTargeting(Id, Id, Geid.Empty, healAmount, targetingSpec);

        Effects = new List<IGameEvent> { healEvent };    }
}