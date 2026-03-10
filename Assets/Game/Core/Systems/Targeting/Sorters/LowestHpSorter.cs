/// <summary>
/// Шаг пайплайна: сортирует кандидатов по возрастанию текущего HP (сначала — с наименьшим HP).
/// Сущности без <see cref="HealthComponent"/> помещаются в конец списка.
/// </summary>
public class LowestHpSorter : ITargetingStep
{
    public void Execute(TargetingContext context)
    {
        var state = context.EventContext.BattleState;
        context.Candidates.Sort((a, b) =>
        {
            int hpA = state.GetEntity(a)?.GetComponent<HealthComponent>()?.Current ?? int.MaxValue;
            int hpB = state.GetEntity(b)?.GetComponent<HealthComponent>()?.Current ?? int.MaxValue;
            return hpA.CompareTo(hpB); // возрастание
        });
    }
}
