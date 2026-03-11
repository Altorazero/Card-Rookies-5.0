using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Карта «Фаербол».
/// Наносит 6 урона случайному противнику. Стоимость: 3 маны и 2 энергии.
/// - Если ресурсов недостаточно — cancelled (нода эффекта не выполняется).
/// - Если нет целей-противников — наносит урон кастующему.
/// </summary>
public class FireballCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Фаербол";
    public string Description => "Наносит 6 урона случайному противнику. Стоимость: 3 маны и 2 энергии.";
    public int ManaCost => 3;
    public int EnergyCost => 2;
    public CardGraphNode CardGraphRootNode { get; }

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

        // Событие траты ресурсов
        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);

        // Событие урона через таргетинг
        var damageEvent = new MassDamageEvent(casterEntityId, casterEntityId, Geid.Empty, 6, enemyTargetSpec, DamageType.Magical);

        // Нода эффекта — тратит ресурсы и наносит урон
        var effectNode = new CardGraphNode(new List<IGameEvent> { spendEvent, damageEvent });

        // Корневая нода — проверяет ресурсы через предикат на ребре
        var rootNode = new CardGraphNode(new List<IGameEvent>());

        // Переход к эффекту только если есть ресурсы
        var hasResources = CompositePredicates.And(
            new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, ManaCost, casterEntityId),
            new EnergyLevelPredicate(ComparisonOperator.GreaterThanOrEqual, EnergyCost, casterEntityId)
        );
        rootNode.TieNode(effectNode, hasResources);

        CardGraphRootNode = rootNode;
    }
}
