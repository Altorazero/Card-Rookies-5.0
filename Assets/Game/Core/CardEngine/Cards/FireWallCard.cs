using System.Collections.Generic;

/// <summary>
/// Карта «Стена огня».
/// В выбранном направлении появляется стена огня, наносящая 3 урона всем противникам
/// по прямой и накладывающая на них эффект горения.
/// Стоимость: 4 маны и 1 энергии.
/// - Ресурсов нет — SpendResourcesEvent отменяется.
/// - Наличие противников в зоне не важно: способность применяется всегда.
/// </summary>
public class FireWallCard : IPlayingCard
{
    public Geid Id { get; } = Geid.New;
    public string Name => "Стена огня";
    public string Description => "Наносит 3 урона всем противникам по прямой и поджигает их. Стоимость: 4 маны, 1 энергии.";
    public int ManaCost => 4;
    public int EnergyCost => 1;
    public IReadOnlyList<IGameEvent> Effects { get; }

    /// <param name="casterEntityId">Кастующая сущность.</param>
    /// <param name="direction">Направление стены огня (единичный гексовый вектор).</param>
    /// <param name="wallLength">Длина линии (по умолчанию 5).</param>
    /// <param name="burnTicks">Количество тиков горения (по умолчанию 3).</param>
    public FireWallCard(Geid casterEntityId, HexCoordinates direction, int wallLength = 5, int burnTicks = 3)
    {
        var lineSpec = new TargetingSpec
            {
                Description = "Enemies in fire wall line",
                TargetRole = SubjectRole.Target
            }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new EnemyTeamFilter()))
            .AddStep(new FilterStep(new HexShapeFilter(casterEntityId, new HexLineShape(direction, wallLength))));

        var burnLineSpec = new TargetingSpec
            {
                Description = "Burn enemies in fire wall line",
                TargetRole = SubjectRole.Target
            }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new EnemyTeamFilter()))
            .AddStep(new FilterStep(new HexShapeFilter(casterEntityId, new HexLineShape(direction, wallLength))));

        var spendEvent = new SpendResourcesEvent(casterEntityId, casterEntityId, ManaCost, EnergyCost);
        var damageEvent = new MassDamageEvent(casterEntityId, casterEntityId, Geid.Empty, 3, lineSpec, DamageType.Magical);
        var burnMassEvent = new FireWallBurnEvent(casterEntityId, casterEntityId, burnLineSpec, 3, burnTicks);

        Effects = new List<IGameEvent> { spendEvent, damageEvent, burnMassEvent };
    }
}
