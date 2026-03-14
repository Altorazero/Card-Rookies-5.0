using System.Collections.Generic;

/// <summary>
/// Карта «Бросок ножей».
/// Наносит 3 урона всем противникам в конусе перед кастером в выбранном направлении.
/// Стоимость: 2 энергии.
/// - Если энергии недостаточно — SpendResourcesEvent отменяется.
/// - Если целей нет — MassDamageEvent fizzled.
/// </summary>
public class KnifeThrowCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Бросок ножей";
    public string Description => "Наносит 3 урона всем противникам в конусе. Стоимость: 2 энергии.";
    public int ManaCost => 0;
    public int EnergyCost => 2;
    public IReadOnlyList<IGameEvent> Effects { get; }

    /// <param name="casterEntityId">Кастующая сущность.</param>
    /// <param name="direction">Направление конуса (единичный гексовый вектор).</param>
    /// <param name="coneRadius">Глубина конуса (по умолчанию 3).</param>
    public KnifeThrowCard(Geid casterEntityId, HexCoordinates direction, int coneRadius = 3)
    {
        var coneSpec = new TargetingSpec
            {
                Description = "Enemies in cone",
                TargetRole = SubjectRole.Target
            }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new EnemyTeamFilter()))
            .AddStep(new FilterStep(new HexShapeFilter(casterEntityId, new HexConeShape(direction, coneRadius))))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));

        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);
        var damageEvent = new MassDamageEvent(casterEntityId, casterEntityId, Geid.Empty, 3, coneSpec, DamageType.Physical);

        Effects = new List<IGameEvent> { spendEvent, damageEvent };
    }
}
