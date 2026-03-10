/// <summary>
/// Шаг пайплайна: сортирует кандидатов по гексагональному расстоянию от <see cref="OriginEntity"/>
/// (сначала — ближайшие).
/// Сущности без <see cref="HexComponent"/> или отсутствующий origin помещаются в конец списка.
/// </summary>
public class NearestSorter : ITargetingStep
{
    /// <summary>Сущность, от которой измеряется расстояние.</summary>
    public Geid OriginEntity { get; }

    public NearestSorter(Geid originEntity)
    {
        OriginEntity = originEntity;
    }

    public void Execute(TargetingContext context)
    {
        var state = context.EventContext.BattleState;
        var originHex = state.GetEntity(OriginEntity)?.GetComponent<HexComponent>();

        if (originHex == null)
            return; // нет HexComponent у источника — порядок не меняем

        context.Candidates.Sort((a, b) =>
        {
            int dA = GetDist(state, a, originHex.Coordinates);
            int dB = GetDist(state, b, originHex.Coordinates);
            return dA.CompareTo(dB);
        });
    }

    private static int GetDist(BattleState state, Geid id, HexCoordinates origin)
    {
        var hex = state.GetEntity(id)?.GetComponent<HexComponent>();
        return hex == null ? int.MaxValue : HexCoordinates.Distance(origin, hex.Coordinates);
    }
}
