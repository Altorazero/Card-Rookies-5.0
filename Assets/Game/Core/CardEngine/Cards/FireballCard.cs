using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Карта «Фаербол».
/// Наносит 6 урона случайному противнику. Стоимость: 3 маны и 2 энергии.
/// - Если ресурсов недостаточно — SpendResourcesEvent отменяется, урон не наносится.
/// - Если нет целей-противников — наносит урон кастующему.
/// </summary>
public class FireballCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Фаербол";
    public string Description => "Наносит 6 урона случайному противнику. Стоимость: 3 маны и 2 энергии.";
    public int ManaCost => 3;
    public int EnergyCost => 2;
    public IReadOnlyList<IGameEvent> Effects { get; }

    public FireballCard(Geid casterEntityId)
    {
        // Таргетинг на случайного противника
        var enemyTargetSpec = new TargetingSpec
            {
                Description = "Random enemy or self",
                TargetRole = SubjectRole.Target
            }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new EnemyTeamFilter()))
            .AddStep(new RandomSorter())
            .AddStep(new TakeSorter(1))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onMet: new CommitAndStopAction(),
                onNotMet: new AlternativeEffectAction(ctx =>
                {
                    // Нет противников — наносим урон кастующему
                    Debug.Log("[FireballCard] No enemies found, targeting caster.");
                    return new SingleDamageEvent(casterEntityId, casterEntityId, casterEntityId, 6, DamageType.Magical);
                })));

        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);
        var damageEvent = new MassDamageEvent(casterEntityId, casterEntityId, Geid.Empty, 6, enemyTargetSpec, DamageType.Magical);

        Effects = new List<IGameEvent> { spendEvent, damageEvent };
    }
}
