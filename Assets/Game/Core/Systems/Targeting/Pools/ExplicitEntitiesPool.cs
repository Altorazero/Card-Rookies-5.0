using System.Collections.Generic;

/// <summary>
/// Шаг-пул: добавляет в список кандидатов конкретный перечень сущностей.
/// Сущности, которых нет в BattleState, молча пропускаются.
/// Дубликаты не добавляются.
/// </summary>
public class ExplicitEntitiesPool : ITargetingStep
{
    private readonly IReadOnlyList<Geid> _entities;

    public ExplicitEntitiesPool(params Geid[] entities)
    {
        _entities = entities;
    }

    public ExplicitEntitiesPool(IEnumerable<Geid> entities)
    {
        _entities = new List<Geid>(entities);
    }

    public void Execute(TargetingContext context)
    {
        var state = context.EventContext.BattleState;
        foreach (var id in _entities)
        {
            if (state.GetEntity(id) != null && !context.Candidates.Contains(id))
                context.Candidates.Add(id);
        }
    }
}
