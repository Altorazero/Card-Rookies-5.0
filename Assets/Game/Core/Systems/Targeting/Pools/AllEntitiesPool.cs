/// <summary>
/// Шаг-пул: добавляет в список кандидатов все сущности из BattleState.
/// Дубликаты пропускаются.
/// </summary>
public class AllEntitiesPool : ITargetingStep
{
    public void Execute(TargetingContext context)
    {
        foreach (var id in context.EventContext.BattleState.Entities.Keys)
        {
            if (!context.Candidates.Contains(id))
                context.Candidates.Add(id);
        }
    }
}
