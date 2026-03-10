/// <summary>
/// Фильтр, который пропускает все цели
/// </summary>
public class AlwaysValidFilter : ITargetFilter
{
    public bool IsTargetValid(Geid target, EventContext context)
    {
        return context?.BattleState?.GetEntity(target) != null;
    }
}