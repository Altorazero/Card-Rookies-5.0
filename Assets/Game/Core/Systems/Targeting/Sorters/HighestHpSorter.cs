/// <summary>
/// Шаг пайплайна: сортирует кандидатов по убыванию текущего HP (сначала — с наибольшим HP).
/// Сущности без <see cref="HealthComponent"/> помещаются в конец списка.
/// </summary>
public class HighestHpSorter : ITargetingStep
{
    public void Execute(TargetingContext context)
    {
        var state = context.EventContext.BattleState;
        context.Candidates.Sort((a, b) =>
        {
            int hpA = state.GetEntity(a)?.GetComponent<HealthComponent>()?.Current ?? 0;
            int hpB = state.GetEntity(b)?.GetComponent<HealthComponent>()?.Current ?? 0;
            return hpB.CompareTo(hpA); // убывание
        });
    }
}
